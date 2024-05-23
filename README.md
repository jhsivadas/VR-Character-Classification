The code contained in this folder contains most of the information required for setting up the Unity Folder. Below is a list of the files and their purpose:
1. Unity\ Room/Assets/Scripts/Drawing.cs: This is the major workhorse of the C# scripts. This file records the data, manages most of the interactions in the Unity Room
2. Unity\ Room/Assets/Scripts/Marker.cs: Marker.cs controls the marker’s position, and when it should move.
3. Unity\ Room/Assets/Scripts/GoogleCloud.cs: This is a deprecated file, whose functions have since all been integrated into the Drawing.cs file. 
4. Unity\ Room\Assets/SampleScene.unity: This is the actual code for the unity room.
5. Python\ transform.py: This is the data conversion code that reads from Google Cloud Storage and converts VR data to EMNIST representation data
6. Python\ model.py: Code for training our CNN using tensorflow
7. Pyhton \modeltest.py: Code for training model and viewing results (summary, matrix, etc..)

When setting up the code in a headset, there is one extra step that needs to be taken. Once you have finished building the unity program, you need to access the VR headset’s file system add the correct google credentials. The code will also need to be edited as well in the following ways, to include your own google credentials file. This is what allows GCP to authenticate the specific request. NOTE: Our google credentials are private and not uploaded to the GitHub!!!