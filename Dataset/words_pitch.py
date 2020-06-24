import librosa
import os

persons = ['AlexM', 'AlexP', 'Jonas', 'Lara']
words = ['create', 'delete', 'select', 'color', 'move', 'cube', 'sphere', 'plane', 'red', 'green', 'blue', 'white']


for dirname, _, filenames in os.walk('customUnicornn'):
    for filename in filenames:
        path = os.path.join(dirname, filename)

        if path.count("pitched-pitched"):
            os.remove(path)

        """if path.endswith('.wav'):
            try:
                y, sr = librosa.load(path, sr=44100)
                y_shifted = librosa.effects.pitch_shift(y, sr, n_steps=2)
                librosa.output.write_wav(path.replace(".wav","-pitched.wav"), y_shifted, sr)
                print(path.replace(".wav","-pitched.wav"))
            except FileNotFoundError:
                print("File doesn't exist: ", path)"""

"""
for name in persons:

    for key in words:
        filename2 = name + '-' + key
        new_filename2 = name + '-' + key

        for i in range(1,55):
            if i <= 9:
                filename3 = filename2 + '-00' + str(i) + '.wav'
                new_filename3 = new_filename2 + '-00' + str(i) + '-pitched.wav'
            else:
                filename3 = filename2 + '-0' + str(i) + '.wav'
                new_filename3 = new_filename2 + '-0' + str(i) + '-pitched.wav'

            print("customUnicornn/" + key + "/" + filename3)
            try:
                y, sr = librosa.load("customUnicornn/" + key + "/" + filename3, sr=44100)
                y_shifted = librosa.effects.pitch_shift(y, sr, n_steps=2)
                librosa.output.write_wav("customUnicornn/" + key + "/" + new_filename3, y_shifted, sr)
            except FileNotFoundError:
                print("File doesn't exist: ", filename3)
"""