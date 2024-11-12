# Terminal OS
terminal_os is a terminal game written in the Godot engine using .NET 8 and C#.

![image](https://github.com/user-attachments/assets/4d4f92c8-3938-4590-8a6f-7b5dafccfe3b)

## Features
- View all the commands for terminal_os using the `commands` command
- Get help about any command using the `help` command
- Change the terminal colors using the `color` command
- See the contents of any directory using the `list` command
- Change your current directory using the `change` command
- View a file using the `view` command
- Launch the file editor and edit a file using the `edit` command
- Make a file in the current directory using the `makefile` command
- Make a directory in the current directory using the `makedirectory` command
- List the hardware using the `listhardware` command
- View the permissions of a file or directory using the `viewpermissions` command
    - Use the `help viewpermissions` command to get information about permission bits
- Change the permissions of a file or directory using the `changepermissions` command
    - Use the `help changepermissions` command to get information about permission bits
- Get the current date and time using the `now` command
    - Get the current date using the `date` command
    - Get the current time using the `time` command
- Get the current networking information using the `network` command
    - Use the `help network` command to get information about valid arguments
- Add new colors to the terminal by using `edit` on the `/system/config/color.conf` file, and adding a name and hex value
- Contextual autocomplete for commands by pressing the tab key
- Save your game by using the `save` command
- Exit the terminal using the `exit` command (but not before using `save` to save your game)