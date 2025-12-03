# Cat Righting Reflex - Deep Reinforcement Learning

## Project Overview

This project implements a physics-based cat righting reflex using Deep Reinforcement Learning in Unity. The agent learns to flip from arbitrary mid-air orientations to land belly-down, purely through internal momentum redistribution.

## Requirements

#### 1. Unity 6.0

Version 6000.0.62f1 is recommended.

#### 2. ML-Agents 4.0.0

```
Window → Package Manager, install ML-Agents from Unity Registry
```

#### 3. Python 3.9 with `mlagents` package (only required for training, not for demo)

```shell
conda create -n catflip python=3.9 -y
conda activate catflip
pip install mlagents
```

## Project Structure

```shell
catflip/
├── Assets/
│   ├── Cat.onnx                    # Trained policy network
│   ├── Lowpoly Cat Model...fbx     # Rigged cat model
│   ├── Scenes/                     
│   │   ├── ManualTestDemo.unity    # Scene: Manual joint control
│   │   ├── TrainedCatDemo.unity    # Scene: Trained agent demo
│   │   └── CatTraining.unity       # Scene: Training environment (16 cats)
│   ├── Scripts/                    
│   │   ├── CatAgent.cs             # RL agent implementation
│   │   ├── CatPhysicsSetup.cs      # Physics config
│   │   ├── ManualJointTest.cs      # Allow manual joint control
│   │   ├── CameraControl.cs        # Camera navigation
│   │   └── TrainingAreaManager.cs  # Training area setup
│   ├── Prefabs/
│   │   └── CatAgent.prefab
│   └── Materials/                  
├── config/                         
│   └── cat_config.yaml             # DRL training config
├── ProjectSettings/                
└── Packages/                       
```

## Quick Start: View the Trained Agent

1. **Open the project in Unity**
2. **Open scene**: `Assets/Scenes/TrainedCatDemo.unity`
3. **Press Play**
4. **Interact**:
   - The cat starts in a random orientation
   - Watch it automatically flip to belly-down
   - In **Scene view**, manually rotate the **white capsule physics model** (not the cat visual model)
   - See its real-time adaptation

## Scene Descriptions

### 1. ManualTestDemo.unity

Demonstrates manual control of the three joint degrees of freedom.

**Controls**:
- `1` / `2`: DOF 1 (Spine Bending)
- `3` / `4`: DOF 2 (Lateral Bending)
- `5` / `6`: DOF 3 (Axial Twist)
- `WASD`: Move camera
- `Q` / `E`: Camera up/down
- `Ctrl + Mouse Drag`: Rotate camera view

### 2. TrainedCatDemo.unity
Shows the trained RL agent automatically flipping from random poses.

**What to observe**:

- Smooth, coordinated motion using all three DOFs
- Completes flip in seconds
- Both physics capsules and rigged cat model move together

**Testing adaptability**:

1. Pause (Space)
2. In Scene view, select the white physical cat model and manually rotate it
3. Resume—agent adapts in real-time

### 3. CatTraining.unity

Training environment with 16 parallel cat agents.

**Note**: This scene requires ML-Agents to be properly configured. Without training mode, the cats will use the trained policy from `Cat.onnx`.

## Manual Training

If you want to retrain the agent:

1. **Install Python and the dependency as stated in Requirements**
2. **Run training**:
```bash
mlagents-learn config/cat_config.yaml --run-id=catflip_v1
```

3. **Open Unity and press Play to start training**
3. **Training details**:
   - 16 parallel environments
   - Episodes: 3 seconds (150 timesteps at 50Hz)
   - Converges in ~2 hours on CPU
   - Checkpoint saved every 50k steps

## Citation

If you use this work, please cite:

> Cui, P. (2025). Teaching AI the Cat Righting Reflex: A Deep Reinforcement Learning Approach. CMPT 766 Computer Animation Course Project, Simon Fraser University.

## Acknowledgements

- Rigged cat model: https://www.cgtrader.com/items/4590731/download-page
- Unity ML-Agents: https://github.com/Unity-Technologies/ml-agents
- Course instructor: Professor KangKang Yin

## Contact

For questions or issues, contact: https://github.com/PkuCuipy