import librosa
import numpy as np
import soundfile as sf

file_path = "test.wav"
bufferSize = 1024
threshhold = 15
frontWrapperTimer = 0.1
endWrapperTimer = 0.4
samples = []


def HandleAudioBuffer():
    bufferCounter = 0
    bufferLoudness = 0
    allBuffers = []
    buffer = []

    for i in samples:
        if bufferCounter == 0:
            buffer = []

        if bufferCounter < bufferSize:
            bufferLoudness += abs(i)
            bufferCounter += 1
            buffer.append(i)

        if bufferCounter >= bufferSize:
            convertedBuffer = Buffer(buffer, bufferLoudness)
            allBuffers.append(convertedBuffer)
            bufferCounter = 0
            bufferLoudness = 0
    FindSeries(allBuffers)

def FindSeries(buffers):
    foundSeries = False
    seriesCounter = 0

    series = []

    counter = 0
    for i in buffers:
        if CheckFutureBuffer(counter, buffers):
            foundSeries = True
            series.append(i)
        elif foundSeries:
            WrapUpSeries(counter, series, buffers, seriesCounter)
            foundSeries = False
            series = []
            seriesCounter += 1
        counter += 1

def CheckFutureBuffer(index, buffers):
    trueCounter = 0
    falseCounter = 0
    for i in range(index, index+25):
        if len(buffers) > i:
            if buffers[index].bufferLoudness > threshhold:
                trueCounter += 1
            else:
                falseCounter += 1

    if trueCounter > falseCounter:
        return True
    else:
        return False

def WrapUpSeries(index, series, buffers, seriesCounter):
    finalBuffering = []
    timeFactor = bufferSize / 48000
    startingPoint = index - len(series)
    frontWrapper = []
    counter = 0
    while counter * timeFactor < frontWrapperTimer:
        if(startingPoint - counter > 0):
            frontWrapper.append(buffers[startingPoint - counter])

        counter += 1
    frontWrapper.reverse()

    for i in frontWrapper:
        finalBuffering.append(i)

    for i in series:
        finalBuffering.append(i)

    counter = 0
    while counter * timeFactor < endWrapperTimer:
        if(startingPoint - counter):
            finalBuffering.append(buffers[startingPoint + counter])
        counter += 1

    if seriesCounter == 0:
        #firstWord = DeserializedSeries(finalBuffering)
        print(1)s
    elif seriesCounter == 1:
        secondWord = DeserializedSeries(finalBuffering)
        print(2)

def DeserializedSeries(series):
    deSeries = []
    for i in series:
        for j in i.value:
            deSeries.append(j)

    print(len(deSeries))
    sf.write('second.wav', deSeries, 22100, subtype='PCM_24')
    return deSeries

class Buffer:
    def __init__(self, values, loudness):
        self.value = values
        self.bufferLoudness = loudness


if __name__ == '__main__':
    samples, sr = librosa.load(file_path)
    HandleAudioBuffer()
