import librosa
import os
import random
import numpy as np
import matplotlib.pyplot as plt
import torch
import torch.nn as nn
import torchvision

import collections
from torchsummary import summary

from tqdm import tqdm

import VGG
import onnxruntime as ort

durationOfAudioSample = 1


# randomly change amplitude of audio in given range
def manipulateAmplitude(samples, amplitude_range):
    samples = samples * random.uniform(*amplitude_range)
    return samples

# randomly change speed (pitch) of audio within maximum scale
def manipulateSpeed(samples, max_scale):
    scale = random.uniform(-max_scale, max_scale)
    speed_factor = 1.0 / (1 + scale)
    samples = np.interp(np.arange(0, len(samples), speed_factor), np.arange(0,len(samples)), samples).astype(np.float32)
    return samples

# randomly add background noise
def backgroundNoise(samples, standard_deviation):
    noise = np.random.normal(0, standard_deviation, samples.shape)
    #print(noise)
    samples += noise
    return samples

#randomly shift audio in time
def manipulateShift(samples, sample_rate, max_shift_seconds):
    max_shift = sample_rate * max_shift_seconds
    shift = random.randint(-max_shift, max_shift)
    minimum = -min(0, shift)
    maximum = max(0, shift)
    samples_temp = np.pad(samples, (minimum, maximum), "constant")
    samples = samples[:len(samples) - minimum] if minimum else samples[maximum:]
    return samples

# randomly change length of audio by stretching
def manipulateLength(samples, max_scale):
    scale = random.uniform(-max_scale, max_scale)
    samples = librosa.effects.time_stretch(samples, 1+scale)
    return samples


def wave2image(file):

    samples, sample_rate = librosa.core.load(file, sr=16000)

    # Audio Manipulation
    #samples = manipulateAmplitude(samples, (0.7, 1.1))
    #samples = manipulateSpeed(samples, 0.2)
    #samples = backgroundNoise(samples, 0.01)
    #samples = manipulateShift(samples, sample_rate, 0.2)
    #samples = manipulateLength(samples, 0.2)

    # correct length of audiosample to fix length
    fixed_sample_length = int(durationOfAudioSample * sample_rate)
    if fixed_sample_length < len(samples):      #truncate
        samples = samples[:fixed_sample_length]
    elif fixed_sample_length > len(samples):    # add silence
        samples = np.pad(samples, (0, fixed_sample_length - len(samples)), "constant")

    # Create MelSpectrum
    s = librosa.feature.melspectrogram(samples, sr=sample_rate, n_mels=32)
    mel = librosa.power_to_db(s, ref=np.max)
    return mel


device = "cuda:0" if torch.cuda.is_available() else "cpu"
print("Device:", device)

model = VGG.VGG(16)

model.load_state_dict(torch.load("Weights/bestWeights.pth"))
# model = model.to(device)
model.eval()

dummy_input = torch.randn(1, 1, 32, 32, device="cpu")
torch.onnx.export(model.to("cpu"), dummy_input, "VGG-Test.onnx", verbose=True)

ort_session = ort.InferenceSession('VGG-Test.onnx')
print(ort_session)


test = ["test/zero.wav",
        "test/one.wav",
        "test/two.wav",
        "test/three.wav",
        "test/four.wav",
        "test/five.wav",
        "test/six.wav",
        "test/seven.wav",
        "test/eight.wav",
        "test/nine.wav"]

for i in test:
    _image = wave2image(i)

    _min = np.min(_image)
    _image -= _min
    _max = np.max(_image)
    _image /=_max

    input = torch.Tensor(_image).unsqueeze(0).unsqueeze(0) #.to(device)
    print(model(input).argmax(1))

    ort_session = ort.InferenceSession('VGG-Test.onnx')
    outputs = ort_session.run(None, {"input.1":input.numpy()})
    print(outputs[0].argmax(1))

# print(model.features[0].weight)

"""
2.1475e-02, -8.9793e-02,  1.2333e-02],
          [ 4.6871e-02, -2.7569e-02,  1.0232e-03],
          [ 1.0249e-01, -4.1800e-02,  4.2325e-02]]],

"""