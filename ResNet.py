import torch
import torch.nn as nn

import torchvision.models as models



model = models.resnet34()
model.conv1 = nn.Conv2d(1, 64, kernel_size=(7, 7), stride=(2, 2), padding=(3, 3), bias=False)
print(model)

dummy_input = torch.randn(16, 1, 32, 32, device="cpu")
torch.onnx.export(model.to("cpu"), dummy_input, "ResNet-Test.onnx", verbose=True)
