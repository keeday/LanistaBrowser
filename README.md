
![Logo](https://raw.githubusercontent.com/keeday/LanistaBrowser/main/logo.png)




Lanista Browser is a specialized tool for playing the web-based game [https://beta.lanista.se/](https://beta.lanista.se/)

**The Browser is in Beta, there will be bugs!**

**You can find the changelog [here](https://github.com/keeday/LanistaBrowser/blob/main/changelog.MD).**

## Main Features

**Theory Crafting**

Craft and build tactics, equip weapons and other equipment, and display final stats after race and equipment bonuses.

**Items Database** 

Provides a fully searchable database for all of Lanista's weapons, shields, equipment, and consumables with advanced filters. Mark specific items as favorites to find them easier (e.g., in Theory Crafter).

**Timers and Reminders**

Create and set Timers/Reminders.

**Data Sync** (Upcoming)

Once the Android version is released, I will look in to the possibilites of being able to sync your saved tactics between your clients.

## Installing

1. [Go here](https://github.com/keeday/LanistaBrowser/releases) and download the latest release.
2. Extract the downloaded zip file to a folder.
3. Run "LanistaBrowser.exe" to launch the application.
4. **Note -** Antivirus software might block the browser.

## Updating

Download the latest release from [here](https://github.com/keeday/LanistaBrowser/releases) and extract it in the folder you already have the Browser in, and just replace all files.

## Requirements

On Windows the browser utilizes [Microsoft's Webview](https://learn.microsoft.com/en-us/microsoft-edge/webview2/).

* Webview will be pre-installed if you're on Windows 11.
* If you're on Windows 10 or earlier, you probably have it, but might need to install it.

## FAQ

**Will the browser be available for other platforms?**

Yes, an Android version is planned. The framework (Avalonia) allow builds for Mac, iOS, and Linux. However, building, testing, and releasing for these platforms is currently not something I am capable of, as I do not use any of these platforms. I am happy to try to build for these platforms, but I will need assistance in testing before I publish. Please contact me if you're interested in helping.

**Do you have a roadmap?**

The closest thing to a roadmap is my [Trello Board](https://trello.com/b/UTABzwpt/lanistabrowser).

**I have an idea/found a bug!**

Great! You can create an issue post [Here](https://github.com/keeday/LanistaBrowser/issues). Optionally, you can find me lurking in Lanista's official Discord channel (@Keeday).

**How do I use the Theory Crafter?**

I am currently working on a guide. Check back here later.

**What data is stored, and how is it stored?**

The only data that is being stored is the tactics you create. And that is being stored locally on your PC in a .db file (SQLite). You find this file under **/documents/LanistaBrowser** after you have started it up for the first time.

**Will I loose my saved tactics and favorites if I update the Browser?**

No. As long as you don't manually delete the **/documents/LanistaBrowser** folder, your tactics and favorites will be saved even if you delete/replace/update the Browser files.

**Can I help develop the Browser?**

Yes! If you have experience with C# and want to help develop the Browser, please contact me and we can discuss further!
