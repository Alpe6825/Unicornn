import torch
import torch.nn as nn

input_size = 32
sequence_length = 32
num_layers = 2
hidden_size = 256
num_classes = 12
learning_rate = 0.001
batch_size = 128
num_epochs = 2
device = "cpu"

class RNN(nn.Module):
    def __init__(self, input_size, hidden_size, num_layers, num_classes):
        super(RNN, self).__init__()
        self.hidden_size = hidden_size
        self.num_layers = num_layers
        self.rnn = nn.RNN(input_size, hidden_size, num_layers, batch_first=True)
        self.fc = nn.Linear(hidden_size*sequence_length, num_classes)
        self.isRNN = True

    def forward(self, x):
        x = x.to(device=device).squeeze(1)
        print(x.size(0))
        h0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(device)   # initialize hidden state

        # Forward Prop
        out, _ = self.rnn(x, h0)
        out = out.reshape(out.shape[0], -1)     # reshape to 256
        out = self.fc(out)      # pass to linear layer
        return out

model = RNN(input_size, hidden_size, num_layers, num_classes).to(device)
print(model)

dummy_input = torch.randn(128, 1, 32, 32, device=device)
torch.onnx.export(model.to("cpu"), dummy_input, "Test.onnx", verbose=True)
