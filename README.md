# PatchKit Securing The Game By License Key

In this repository you can find PatchKit Securing The Game By License Key project.
This package allows you to check the license key from the app_data.json file created by the [Patchkit Launcher](http://docs.patchkit.net/launcher_overview.html).

## Installation steps

1. Download [patchkit-securing-by-the-license-key.unitypackage](https://github.com/patchkit-net/patchkit-securing-the-game-by-license-key/releases/latest) or [source files](https://github.com/Szczyrk/patchkit-securing-the-game-by-license-key/archive/master.zip)(go to step 4).
2. Open downloaded package choosing **Assets/Import Package/Custom Package** in the Unity menu.
3. Confirm importing all the assets by clicking the Import button.
4. [Log in](https://panel.patchkit.net/users/login_form) or if you don't have a PatchKit account yet, it is time to [create it](https://panel.patchkit.net/users/register).
5. To get the App Secret, you need to have an application created ([how to do it?](http://docs.patchkit.net/getting_started.html)).
6. If you already have one - copy the Secret from the application overview.
7. Put the prefab **Assets/PatchKit/Securing by the license key/Prefabs/PatchKitLicenseKey** on your first scene.
8. Paste copied Secret into App Secret field in the prefab parameters.
9. Select the [Action mode](#ActionMode) as you like.
10. Now you can build your game.
11. Archive your build to a .zip file.
12. Upload zip file as a new version of the PatchKit application.
13. Enable license keys for your application.
14. Create a New Key Collection.
15. That's it! You can now download and distribute the Launcher and license keys.

## ActionMode

### Time Stop Start
In this mode the [Time.timeScale](https://docs.unity3d.com/ScriptReference/Time-timeScale.html) parameter was used. The timeScale is a scale at which time passes. When game is stated than timeScale is set to zero, the game is basically paused if all your functions are frame rate independent. When the user enters the correct key then timeScale is 1.0 and time passes as fast as realtime and game will work. 

**Problems**
This action mode does not stop the possibility of clicking on the buttons.

### Enables Selected Objects
In this mode list of Objects is being created and when the game starts, the objects become disabled. When the user enters the correct key then objects become included.

### Load New Scene
In this mode choose the Scene from Project that will start when the user enters the correct key.

### Callback Entry And Exit
In this mode include the following callbacks: 
**OnEntry** when the game starts,
**OnExit** when the user enters the correct key.
