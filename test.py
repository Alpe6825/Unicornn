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

import sys
np.set_printoptions(threshold=sys.maxsize)


durationOfAudioSample = 1



def wave2image(file):

    samples, sample_rate = librosa.core.load(file, sr=16000)

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

"""model = VGG.VGG(16)

model.load_state_dict(torch.load("Weights/bestWeights.pth"))
# model = model.to(device)
model.eval()

dummy_input = torch.randn(1, 1, 32, 32, device="cpu")
torch.onnx.export(model.to("cpu"), dummy_input, "VGG-Test.onnx", verbose=True)
"""

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
        "test/nine.wav",
        "test/forward.wav",
        "test/back.wav",
        "test/left.wav",
        "test/right.wav",
        "test/up.wav",
        "test/down.wav"]

for i in test:
    _image = wave2image(i)

    _min = np.min(_image)
    _image -= _min
    _max = np.max(_image)
    _image /=_max

    input = torch.Tensor(_image).unsqueeze(0).unsqueeze(0) #.to(device)
    # print(model(input).argmax(1))

    ort_session = ort.InferenceSession('VGG-Test.onnx')
    outputs = ort_session.run(None, {"input.1":input.numpy()})
    print(outputs[0].argmax(1))

# print(model.features[0].weight)

_image = wave2image("test/two.wav")
_min = np.min(_image)
_image -= _min
_max = np.max(_image)
_image /=_max

print(_image.reshape(-1))