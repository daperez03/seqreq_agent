---
noteId: "1f1fdfc041d811f1bef525f5f50710e9"
tags: []

---

# IVA-3: Generic Intelligent Virtual Agent

## Overview

IVA-3 is a specialized Unity-based Intelligent Virtual Agent featuring a generic avatar designed for focused communication and basic interaction capabilities. This streamlined implementation emphasizes core conversational features with enhanced audio processing and simplified animation systems, making it ideal for resource-constrained environments and specialized communication applications.

## Technical Specifications

### Unity Version

- **Unity 6000.3.11f1** (Latest Unity 6 LTS)
- Universal Render Pipeline (URP) 17.3.0

### Core Features

#### Avatar Characteristics

- **Gender**: Male
- **Avatar Model**: `avatar.fbx` - Universal 3D rigged character model
- **Design Philosophy**: Neutral appearance for broad accessibility
- **Facial Animation**: Focused on essential communication expressions
- **Audio Integration**: Advanced lip syncing with enhanced control features

#### Streamlined Expression System

IVA-3 implements a focused set of two primary communication states optimized for efficiency:

1. **Advanced Talking Animation** (`talking.cs`)
   - **Enhanced Audio Control**: Unique AudioSource management
   - **Animation Parameter Control**: `IsAudioPlaying` parameter for state management
   - **Advanced Blend Shape Control**: Eye blend shape avoidance system
   - **Separate Audio Source Creation**: Independent audio management per state
   - **Animator Transition Control**: Automated state machine management
   - **Real-time Audio Analysis**: 512-point FFT spectrum analysis
   - **Multiple Viseme Support**: Full phoneme range (A, E, I, O, U + consonants)
   - **Enhanced Sensitivity**: Configurable audio responsiveness (1.5x default)
   - **Superior Smoothness**: 8x interpolation for fluid lip movements
   - **Advanced Phoneme Detection**: M, B, P consonant recognition (85% intensity)

2. **Essential Blink Animation** (`blink.cs`)
   - Natural eye blinking patterns
   - Automatic eyelid coordination
   - Minimal resource consumption
   - Optimized timing algorithms

#### Assets Structure

```
Assets/
├── Avatar/
│   ├── Animations/     # Focused animation scripts (talking, blink)
│   ├── Audio/          # Enhanced audio processing assets
│   ├── Materials/      # Universal PBR materials
│   ├── Textures/       # Generic texture assets
│   └── avatar.fbx      # Universal character model
├── Scenes/
│   └── SampleScene.unity
└── Settings/           # Optimized project configurations
```

## Technical Implementation

### Specialized Animation Pipeline

Streamlined approach focusing on core communication needs:

- **Minimal Animation Set**: Only essential expressions (talking, blinking)
- **Enhanced Talking System**: Advanced audio analysis and lip sync
- **Resource Optimization**: Reduced memory footprint and CPU usage
- **Modular Architecture**: Easy integration into existing systems

### Advanced Audio-Visual Synchronization

IVA-3 features the most sophisticated talking animation system:

- **Independent Audio Management**: Separate AudioSource creation for isolation
- **Animator Integration**: Direct control of animation parameters
- **Blend Shape Protection**: Avoids interference with eye animations
- **Enhanced Viseme Control**: Separate intensity settings for each phoneme type
- **Audio Delay Support**: Synchronized speech timing with visual cues
- **Real-time Parameter Updates**: Dynamic animation state control

## Use Cases

### Resource-Constrained Environments

- Mobile applications requiring virtual agents
- Embedded systems with limited processing power
- Web-based applications with bandwidth constraints
- IoT devices with basic interaction needs

### Specialized Communication Applications

- **Voice Assistants**: Focus on speech and listening capabilities
- **Accessibility Tools**: Simple, clear communication interfaces
- **Tutorial Systems**: Basic instructional avatar needs
- **Customer Service Bots**: Essential interaction capabilities
- **Language Learning**: Pronunciation and speech practice tools

### Development & Integration

- **Prototype Development**: Quick avatar integration for testing
- **API Integration**: Simplified virtual agent for service integration
- **Modular Systems**: Component-based avatar implementation
- **Cross-Platform Applications**: Universal avatar solution

### Educational & Training

- **Basic Virtual Instructors**: Core teaching interaction
- **Communication Training**: Speech and listening practice
- **Accessibility Education**: Simple, clear avatar interactions
- **Language Learning**: Focused on verbal communication

## Technical Advantages

### Enhanced Talking Features

- **Audio Parameter Control**: Real-time animation state management
- **Blend Shape Isolation**: Prevents conflicts with other facial systems
- **Independent Audio Sources**: Clean audio channel management
- **Advanced Phoneme Recognition**: Superior lip sync accuracy
- **Customizable Sensitivity**: Adaptable to different audio sources

### Simplified Implementation

- **Reduced Complexity**: Easier to understand and modify
- **Lower Resource Requirements**: Efficient for constrained environments
- **Focused Feature Set**: Clear separation of concerns
- **Modular Design**: Easy integration and customization

## Getting Started

1. **Open Project**: Load the IVA-3 folder in Unity 6000.3.11f1 or later
2. **Scene Setup**: Open `Assets/Scenes/SampleScene.unity`
3. **Play Mode**: Enter Play Mode to interact with the generic avatar
4. **Audio Configuration**: Set up audio clips for enhanced lip syncing
5. **Parameter Tuning**: Adjust talking animation parameters for optimal performance
6. **Integration**: Use as a component in larger applications

## Dependencies

- Unity AI Navigation Package (2.0.11)
- Unity Input System (1.19.0)
- Universal Render Pipeline (17.3.0)
- Unity Timeline (1.8.11)

## System Requirements

- Unity 6000.3.11f1 or later
- Minimum 2GB RAM (reduced requirements)
- DirectX 11 compatible GPU
- Audio input/output support for communication features

## Configuration Options

### Advanced Talking Parameters

- `createSeparateAudioSource`: Enable independent audio management
- `controlAnimatorTransitions`: Automatic state machine control
- `audioPlayingParameterName`: Custom parameter name for integration
- `avoidEyeBlendShapes`: Prevent eye animation interference
- `lipSyncSensitivity`: Audio responsiveness adjustment
- `smoothness`: Interpolation quality control
