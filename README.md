Issue: On ios bulid, the ar camera background either freezes or it displays a full black background after opening the app


Steps to reproduce issue:

- Clone repo and open it with unity 6000.0.32f1.
- Open build window and on the scene list you will see two scenes. (one that is named "Sample Based Scene" is the one with the described issue, the other one named "BasicImageTracking" is the [unity arfoundation samples basic image tracking example](https://github.com/Unity-Technologies/arfoundation-samples?tab=readme-ov-file#basic-image-tracking) imported to this one and it doesn't have the same issue)
- Make a build with just "Sample Based Scene" and on android you will see app working normally (after getting past the start screen pointing camera at the corresponding image that is set on top of your screen you should see the image getting placed on it) and in ios after getting past the starting screen you will see the camera bcakround either fully black or get stuck after a little bit
- Make a build with just "BasicImageTracking" and you will see on bot ahdroid and ios the unity example working as it should

To me what has been so mind boggling about the problem is, it has no issue on android and both of the scenes have almost identical configurations as far as i can tell... Except my scene sets the ARTrackedImageManager's serializedLibrary field directly and the "ARF XR Origin Set Up" gameobject gets activated after the starting screen..

XR and URP configurations are same, the camera gameobjects are identical. I have also tried different unity versions both unity 6 and unity 22 and the problem still persisted.

I feel like the root of the issue is so basic but so fundemental and very well hidden and i feel very stuck because nothing i did effected this behaviour in any way, except the way it fails (black or freeze) which seems random to me...
