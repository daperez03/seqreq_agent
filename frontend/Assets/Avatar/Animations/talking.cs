using UnityEngine;
using System.Collections;

public class talking : StateMachineBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool useExistingAudioSource = true;
    [SerializeField] private bool createSeparateAudioSource = false; // Create unique AudioSource for this state
    [SerializeField] private bool autoPlayAudioClip = true;
    [SerializeField] private float audioDelay = 0f; // Delay before playing audio
    [SerializeField] private float lipSyncSensitivity = 1.5f;
    [SerializeField] private float smoothness = 8f;
    
    [Header("Animation Control")]
    [SerializeField] private string audioPlayingParameterName = "IsAudioPlaying";
    [SerializeField] private bool controlAnimatorTransitions = true;
    
    [Header("Viseme Intensities")]
    [SerializeField] private float silenceThreshold = 0.01f;
    [SerializeField] private float vowelAIntensity = 80f;    // "ah" sound
    [SerializeField] private float vowelEIntensity = 60f;    // "eh" sound  
    [SerializeField] private float vowelIIntensity = 40f;    // "ee" sound
    [SerializeField] private float vowelOIntensity = 90f;    // "oh" sound
    [SerializeField] private float vowelUIntensity = 70f;    // "oo" sound
    [SerializeField] private float consonantIntensity = 50f; // general consonants
    [SerializeField] private float mBPIntensity = 85f;       // M, B, P sounds
    
    [Header("Blend Shape Control")]
    [SerializeField] private bool avoidEyeBlendShapes = true; // Never touch eye/blink related blend shapes
    
    // Audio analysis
    private AudioSource audioSource;
    private float[] spectrumData = new float[512];
    private float currentVolume = 0f;
    
    // Cached blend shape indices
    private int jawOpenIndex = -1;
    private int mouthAIndex = -1;
    private int mouthEIndex = -1;
    private int mouthIIndex = -1;
    private int mouthOIndex = -1;
    private int mouthUIndex = -1;
    private int mouthPressIndex = -1;
    private int mouthPuckerIndex = -1;
    
    // Current viseme weights
    private float targetJawOpen = 0f;
    private float targetMouthA = 0f;
    private float targetMouthE = 0f;
    private float targetMouthI = 0f;
    private float targetMouthO = 0f;
    private float targetMouthU = 0f;
    private float targetMouthPress = 0f;
    private float targetMouthPucker = 0f;
    
    // Timer for delayed audio playback
    private float audioStartTimer = 0f;
    private bool audioStarted = false;
    
    // Track if we created our own AudioSource (for cleanup)
    private bool createdOwnAudioSource = false;
    private AudioClip originalAudioSourceClip = null;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reset state flags
        audioStartTimer = 0f;
        audioStarted = false;
        createdOwnAudioSource = false;
        
        // Handle AudioSource creation/assignment
        SetupAudioSource(animator);
        
        if (audioSource == null)
        {
            Debug.LogError("Failed to setup AudioSource for lip sync!");
            return;
        }
        
        // Cache blend shape indices
        SkinnedMeshRenderer smr = animator.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null)
        {
            CacheBlendShapeIndices(smr);
        }
        
        // Handle audio playback
        StartAudioPlayback(animator);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Handle delayed audio start
        if (!audioStarted && audioDelay > 0f && audioClip != null && autoPlayAudioClip)
        {
            audioStartTimer += Time.deltaTime;
            if (audioStartTimer >= audioDelay)
            {
                audioSource.Play();
                audioStarted = true;
                
                // Set animator parameter when delayed audio starts
                if (controlAnimatorTransitions)
                {
                    animator.SetBool(audioPlayingParameterName, true);
                }
            }
        }
        
        // Check if audio has finished playing
        if (audioSource != null && audioStarted && controlAnimatorTransitions)
        {
            bool isAudioPlaying = audioSource.isPlaying;
            
            // Update animator parameter based on audio playback status
            animator.SetBool(audioPlayingParameterName, isAudioPlaying);
            
            // If audio stopped but we're still in this state, we can optionally do something
            if (!isAudioPlaying)
            {
                // Audio has finished - the animator transitions should handle moving to next state
                // based on the audioPlayingParameterName parameter being false
            }
        }
        
        if (audioSource == null || !audioSource.isPlaying)
            return;
            
        SkinnedMeshRenderer smr = animator.GetComponent<SkinnedMeshRenderer>();
        if (smr == null || smr.sharedMesh == null)
            return;
        
        // Analyze audio
        AnalyzeAudio();
        
        // Calculate viseme weights based on audio analysis
        CalculateVisemeWeights();
        
        // Apply smooth blend shape transitions
        ApplyLipSyncBlendShapes(smr);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Clear the audio playing parameter
        if (controlAnimatorTransitions)
        {
            animator.SetBool(audioPlayingParameterName, false);
        }
        
        // Reset mouth blend shapes to neutral
        SkinnedMeshRenderer smr = animator.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null)
        {
            ResetMouthBlendShapesOnly(smr);
        }
        
        // Handle audio cleanup
        CleanupAudio(animator);
        
        // Reset state
        audioStartTimer = 0f;
        audioStarted = false;
    }
    
    private void SetupAudioSource(Animator animator)
    {
        if (createSeparateAudioSource)
        {
            // Create a dedicated AudioSource for this state
            GameObject audioObj = new GameObject($"TalkingAudio_{GetInstanceID()}");
            audioObj.transform.SetParent(animator.transform);
            audioSource = audioObj.AddComponent<AudioSource>();
            createdOwnAudioSource = true;
            
            // Configure the new AudioSource
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound by default
        }
        else
        {
            // Get or create shared AudioSource
            audioSource = animator.GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                if (useExistingAudioSource)
                {
                    Debug.LogWarning("No existing AudioSource found. Creating shared one for lip sync.");
                }
                
                // Create shared AudioSource if it doesn't exist
                audioSource = animator.gameObject.AddComponent<AudioSource>();
                
                // Set some reasonable defaults for the new AudioSource
                if (audioSource != null)
                {
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D sound by default
                }
            }
            else
            {
                // Store original clip to restore later (if we're not creating separate AudioSource)
                originalAudioSourceClip = audioSource.clip;
            }
        }
    }
    
    private void StartAudioPlayback(Animator animator)
    {
        if (audioClip != null && autoPlayAudioClip)
        {
            // Stop any current audio first to ensure clean start
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            if (audioDelay > 0f)
            {
                // Setup delayed audio playback
                audioStartTimer = 0f;
                audioStarted = false;
                audioSource.clip = audioClip;
            }
            else
            {
                // Play immediately
                audioSource.clip = audioClip;
                audioSource.Play();
                audioStarted = true;
                
                // Set animator parameter to indicate audio is playing
                if (controlAnimatorTransitions)
                {
                    animator.SetBool(audioPlayingParameterName, true);
                }
            }
        }
        else if (audioClip != null)
        {
            // Just assign the clip but don't auto-play
            audioSource.clip = audioClip;
            audioStarted = false;
        }
        else
        {
            audioStarted = true; // No delay needed for existing audio
            
            // If using existing audio and it's already playing, set the parameter
            if (controlAnimatorTransitions && audioSource.isPlaying)
            {
                animator.SetBool(audioPlayingParameterName, true);
            }
        }
    }
    
    private void CleanupAudio(Animator animator)
    {
        if (audioSource != null)
        {
            // Stop audio if we're managing it and auto-play is enabled
            if (autoPlayAudioClip && audioClip != null)
            {
                audioSource.Stop();
            }
            
            if (createdOwnAudioSource)
            {
                // Destroy the separate AudioSource we created (scheduled for end of frame)
                if (audioSource.gameObject != animator.gameObject)
                {
                    Destroy(audioSource.gameObject);
                }
            }
            else if (originalAudioSourceClip != null)
            {
                // Restore original clip if we were using a shared AudioSource
                audioSource.clip = originalAudioSourceClip;
            }
        }
        
        audioSource = null;
        originalAudioSourceClip = null;
        createdOwnAudioSource = false;
    }
    
    private void ResetMouthBlendShapesOnly(SkinnedMeshRenderer smr)
    {
        // Only reset mouth-related blend shapes, preserve everything else (like blinking)
        int[] indices = { jawOpenIndex, mouthAIndex, mouthEIndex, mouthIIndex, 
                         mouthOIndex, mouthUIndex, mouthPressIndex, mouthPuckerIndex };
        
        foreach (int index in indices)
        {
            if (index >= 0)
            {
                smr.SetBlendShapeWeight(index, 0f);
            }
        }
    }
    
    private void CacheBlendShapeIndices(SkinnedMeshRenderer smr)
    {
        // Try multiple naming conventions for each blend shape
        jawOpenIndex = GetBlendShapeIndexSafely(smr, new string[] {"jawForward", "jawOpen", "Jaw_Forward", "JawOpen"});
        mouthAIndex = GetBlendShapeIndexSafely(smr, new string[] {"viseme_aa", "mouthSmile_L", "A", "Mouth_A"});
        mouthEIndex = GetBlendShapeIndexSafely(smr, new string[] {"viseme_E", "E", "Mouth_E"});
        mouthIIndex = GetBlendShapeIndexSafely(smr, new string[] {"viseme_I", "I", "Mouth_I"});
        mouthOIndex = GetBlendShapeIndexSafely(smr, new string[] {"viseme_O", "O", "Mouth_O"});
        mouthUIndex = GetBlendShapeIndexSafely(smr, new string[] {"viseme_U", "U", "Mouth_U"});
        mouthPressIndex = GetBlendShapeIndexSafely(smr, new string[] {"viseme_PP", "mouthPress_L", "Mouth_Press"});
        mouthPuckerIndex = GetBlendShapeIndexSafely(smr, new string[] {"mouthPucker", "Mouth_Pucker"});
        
        // Debug output if no mouth blend shapes are found
        if (jawOpenIndex < 0 && mouthAIndex < 0 && mouthEIndex < 0)
        {
            Debug.LogWarning("No mouth blend shapes found. Available blend shapes:");
            for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                string name = smr.sharedMesh.GetBlendShapeName(i);
                bool isEyeShape = IsEyeRelatedBlendShape(name);
                Debug.Log($"Blend Shape {i}: {name} {(isEyeShape ? "(EYE - AVOIDED)" : "(AVAILABLE)")}" );
            }
        }
    }
    
    private int GetBlendShapeIndexSafely(SkinnedMeshRenderer smr, string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            // Skip if this looks like an eye-related blend shape
            if (avoidEyeBlendShapes && IsEyeRelatedBlendShape(name))
                continue;
                
            int index = smr.sharedMesh.GetBlendShapeIndex(name);
            if (index >= 0)
                return index;
        }
        return -1;
    }
    
    private bool IsEyeRelatedBlendShape(string blendShapeName)
    {
        if (string.IsNullOrEmpty(blendShapeName)) return false;
        
        string lowerName = blendShapeName.ToLower();
        return lowerName.Contains("eye") || 
               lowerName.Contains("blink") || 
               lowerName.Contains("eyelid") ||
               lowerName.Contains("squint") ||
               lowerName.Contains("wink") ||
               lowerName.Contains("lid");
    }
    
    private void AnalyzeAudio()
    {
        // Get spectrum data from audio
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        
        // Calculate overall volume
        float sum = 0f;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
        }
        currentVolume = sum * lipSyncSensitivity;
    }
    
    private void CalculateVisemeWeights()
    {
        // Reset all targets
        targetJawOpen = 0f;
        targetMouthA = 0f;
        targetMouthE = 0f;
        targetMouthI = 0f;
        targetMouthO = 0f;
        targetMouthU = 0f;
        targetMouthPress = 0f;
        targetMouthPucker = 0f;
        
        // If volume is too low, stay silent
        if (currentVolume < silenceThreshold)
            return;
        
        // Analyze frequency ranges to determine visemes
        float lowFreq = GetFrequencyRange(0, 50);      // Low frequencies (0-50)
        float lowMidFreq = GetFrequencyRange(50, 150);  // Low-mid frequencies (50-150)
        float midFreq = GetFrequencyRange(150, 250);    // Mid frequencies (150-250)
        float highMidFreq = GetFrequencyRange(250, 350); // High-mid frequencies (250-350)
        float highFreq = GetFrequencyRange(350, 512);   // High frequencies (350-512)
        
        // Basic jaw opening based on overall volume
        targetJawOpen = Mathf.Clamp(currentVolume * 100f, 0f, 100f);
        
        // Map frequency ranges to visemes (simplified phoneme detection)
        if (lowFreq > highFreq && lowMidFreq > midFreq)
        {
            // Low frequencies dominant - likely vowel sounds like "O" or "U"
            if (lowFreq > lowMidFreq)
            {
                targetMouthO = Mathf.Min(vowelOIntensity, currentVolume * vowelOIntensity);
            }
            else
            {
                targetMouthU = Mathf.Min(vowelUIntensity, currentVolume * vowelUIntensity);
            }
        }
        else if (midFreq > lowFreq && midFreq > highFreq)
        {
            // Mid frequencies dominant - likely "A" or "E" sounds
            if (lowMidFreq > highMidFreq)
            {
                targetMouthA = Mathf.Min(vowelAIntensity, currentVolume * vowelAIntensity);
            }
            else
            {
                targetMouthE = Mathf.Min(vowelEIntensity, currentVolume * vowelEIntensity);
            }
        }
        else if (highFreq > lowFreq)
        {
            // High frequencies dominant - likely "I" sound or consonants
            if (highMidFreq > highFreq * 0.7f)
            {
                targetMouthI = Mathf.Min(vowelIIntensity, currentVolume * vowelIIntensity);
            }
            else
            {
                // Consonants - use mouth press for plosive sounds
                targetMouthPress = Mathf.Min(consonantIntensity, currentVolume * consonantIntensity);
            }
        }
        
        // Detect potential M/B/P sounds (low frequency spikes)
        if (lowFreq > 0.02f && midFreq < lowFreq * 0.3f)
        {
            targetMouthPress = Mathf.Min(mBPIntensity, currentVolume * mBPIntensity);
            targetJawOpen *= 0.3f; // Close jaw for these sounds
        }
    }
    
    private float GetFrequencyRange(int startIndex, int endIndex)
    {
        float sum = 0f;
        for (int i = startIndex; i < endIndex && i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
        }
        return sum;
    }
    
    private void ApplyLipSyncBlendShapes(SkinnedMeshRenderer smr)
    {
        float deltaTime = Time.deltaTime * smoothness;
        
        // Apply lip sync to mouth blend shapes only (preserves eye animations)
        ApplySmoothBlendShape(smr, jawOpenIndex, targetJawOpen, deltaTime);
        ApplySmoothBlendShape(smr, mouthAIndex, targetMouthA, deltaTime);
        ApplySmoothBlendShape(smr, mouthEIndex, targetMouthE, deltaTime);
        ApplySmoothBlendShape(smr, mouthIIndex, targetMouthI, deltaTime);
        ApplySmoothBlendShape(smr, mouthOIndex, targetMouthO, deltaTime);
        ApplySmoothBlendShape(smr, mouthUIndex, targetMouthU, deltaTime);
        ApplySmoothBlendShape(smr, mouthPressIndex, targetMouthPress, deltaTime);
        ApplySmoothBlendShape(smr, mouthPuckerIndex, targetMouthPucker, deltaTime);
    }
    
    private void ApplySmoothBlendShape(SkinnedMeshRenderer smr, int blendShapeIndex, float targetWeight, float deltaTime)
    {
        if (blendShapeIndex >= 0)
        {
            // Double-check: never touch eye-related blend shapes
            if (avoidEyeBlendShapes)
            {
                string blendShapeName = smr.sharedMesh.GetBlendShapeName(blendShapeIndex);
                if (IsEyeRelatedBlendShape(blendShapeName))
                    return; // Skip eye-related shapes completely
            }
            
            // Apply lip sync to mouth blend shapes
            float currentWeight = smr.GetBlendShapeWeight(blendShapeIndex);
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, deltaTime);
            smr.SetBlendShapeWeight(blendShapeIndex, newWeight);
        }
    }
}
