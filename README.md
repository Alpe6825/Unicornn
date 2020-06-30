# Unicornn

1. Before you run the Unity3D project make sure all requirements are installed
 - Take special care about the versions. Especially *Numba* has to be v0.48 because in v0.50 some functions got deleted
 - Make sure Barracuda is installed via Unity3D PackageManager `Window -> Package Manager` (tested with version 1.0.0)
2. Run Unity3D project in Editor in **PlayMode** (tested with Unity2019.3.13)
3. For further information about the usage see readme.md in Unity project folder

## How-To Unicornn

- To get an overview how our trained models are used in Unity3D see the attached video tutorial in Unity project folder

## Requirements

`pip install -r requirements.txt`

## Data

The dataset must be composed of several sources. Look at [Dataset/README.md](Dataset/README.md) for further information.

## Train

Run the jupyter notebook `train.ipynb`.
