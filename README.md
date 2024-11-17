# Terminal OS
terminal_os is a terminal game written in the Godot engine using .NET 8 and C#.

![image](https://github.com/user-attachments/assets/4d4f92c8-3938-4590-8a6f-7b5dafccfe3b)

## Features
### Informational Commands
- View all the commands for terminal_os using the `commands` command
- Get help about any command using the `help` command
- See the contents of any directory using the `list` command
- List the hardware using the `listhardware` command
- Get the current date and time using the `now` command
    - Get the current date using the `date` command
    - Get the current time using the `time` command

### State Commands
- Save your game by using the `save` command
- Exit the terminal using the `exit` command
- Contextual autocomplete for commands by pressing tab
    - See previous autocomplete result using shift + tab

### User Interface Commands
- Change the terminal colors using the `color` command

### Navigation Commands
- Change your current directory using the `change` command

### Networking Commands
- Get the current networking information using the `network` command
    - Use the `help network` command to get information about valid arguments
- Test responses from any ip address using the `ping` command
    - Use the `help ping` command get information about valid arguments

### File Commands
- View a file using the `view` command
- Launch the file editor and edit a file using the `edit` command
- Make a file in the current directory using the `makefile` command
- Delete a file in the current directory using the `deletefile` command
- Move a file from the current directory using the `movefile` command

### User Commands
- Make a user with the `makeuser` command
- Delete a user with the `deleteuser` command
- View a user group with the `viewgroup` command
- Make a user group with the `makegroup` command
- Delete a user gropu with the `deletegroup` command
- Add a user to a group with the `addusertogroup` command
- Delete a user from a group with the `deleteuserfromgroup` command

### Permissions Commands
- View the permissions of a file or directory using the `viewpermissions` command
    - Use the `help viewpermissions` command to get information about permission bits
- Change the permissions of a file or directory using the `changepermissions` command
    - Use the `help changepermissions` command to get information about permission bits

### Folder Commands
- Make a directory in the current directory using the `makedirectory` command
- Delete a directory in the current directory using the `deletedirectory` command
    - Use the `help deletedirectory` command to get information about valid arguments
- Move a directory from the current directory using the `movedirectory` command

## Configuration
- Modify config values in real-time by using `edit` on any `.conf` file
    - `/system/config/color.conf` to create/remove colors
    - `/users/user/config/user.conf` to modify volume