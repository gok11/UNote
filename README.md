<h1 align="center">UNote</h1>


[日本語ドキュメント (Japanese Documents)](README_JA.md)

UNote is a tool that provides functionality to create and edit notes in Unity, allowing multiple users to comment on a single note.

<img width="70%" src="Doc/readme_01.png" alt="01">

<h2>Features</h2>

* Add, delete, and edit notes from the Note Editor.
* Create notes related to the entire project or specific assets.
* Asset notes can be edited from the Inspector when the corresponding asset is selected.
* You can mark notes as favorites to prioritize their display.
* You can archive notes to limit editing and display.
* Using the search function, you can filter notes by text content or sort them by creation date, among other options.

For an overview of planned features, please refer to [TODO.md](Doc/En/TODO.md).

<h2>Installation</h2>

1. From the menu, select Window > Package Manager.
2. Click the + button and select "Add package from git URL...".
3. Enter https://github.com/gok11/UNote.git?path=/Assets/UNote and press Add.

<h2>Quick Start</h2>

<h3>Setting Your Username</h3>
Set the name that will be visible to other users. Since this name affects editing permissions for notes, be careful not to change it after setting it.

1. From the menu, select Edit > Preferences...
2. Set any name under UNote > Editor Name.

<img width="50%" src="Doc/readme_02.png" alt="01">

<h3>Adding Project Notes and Comments</h3>

1. From the menu bar, select UNote > Note Editor.
2. Display the note addition window by clicking "Add Note..." in the bottom left.
3. Set the note type to Project and click Add New Note.
4. Change the note name from the top of the right pane.
5. Enter text in the comment input field in the right pane, then click the > button or use the shortcut to add a comment.

You can edit the note names and comments you added at any time.

<h3>Adding Asset Notes</h3>

1. Select the asset for which you want to add a note.
2. Add a comment from the note input field.

You can also add notes from the Note Editor, just like with project notes.<br> Adding notes from the Inspector is recommended as it involves fewer steps.

<h2>License</h2>

MIT ([LICENSE](LICENSE))
