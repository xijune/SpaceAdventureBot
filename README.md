# SpaceAdventureBot

SpaceAdventureBot is an automated bot designed to interact with the Space Adventure game within the Telegram app. It uses OpenCV for image recognition and SharpAdbClient for interacting with an Android device via ADB (Android Debug Bridge).

## Features

- **Automated App Launching**: Starts the Telegram app and launches the Space Adventure game.
- **Daily Activity Check**: Checks and performs daily activities within the game.
- **Anti-Bot Measures**: Detects and bypasses anti-bot measures.
- **Fuel Management**: Checks fuel levels and refuels if necessary.
- **Coin Collection**: Automatically collects coins.
- **Spin Feature**: Interacts with the spin feature in the game.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022
- Android device with ADB enabled
- Required NuGet packages:
  - `Microsoft.ML`
  - `Microsoft.ML.FastTree`
  - `OpenCvSharp4`
  - `OpenCvSharp4.runtime.win`
  - `SharpAdbClient`

## Installation

1. Clone the repository:

    git clone https://github.com/yourusername/SpaceAdventureBot.git
    cd SpaceAdventureBot

2. Open the solution in Visual Studio 2022.

3. Restore the NuGet packages:
    
    dotnet restore

## Configuration

Ensure that your Android device is connected and ADB is enabled. Modify the `Constants` class to include the correct package names and activity names for the Telegram app and Space Adventure game.

## Usage

1. Connect your Android device via USB and ensure ADB is enabled.

2. Run the bot:
    
    dotnet run --project SpaceAdventureBot

3. The bot will start the Telegram app, launch the Space Adventure game, and perform the automated tasks.

## Project Structure

- **Bot.cs**: Contains the main logic for the bot, including methods for starting the app, performing daily activities, managing fuel, collecting coins, and interacting with the spin feature.
- **Device.cs**: Contains methods for interacting with the Android device via ADB, such as taking screenshots, executing commands, and simulating touch events.
- **SpaceAdventureBot.csproj**: Project file containing the dependencies and target framework.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [OpenCvSharp](https://github.com/shimat/opencvsharp) for image recognition.
- [SharpAdbClient](https://github.com/quamotion/madb) for ADB interactions.

## TODO

- **Improve Image Recognition**: Enhance the accuracy and speed of image recognition using advanced techniques or pre-trained models.
- **Error Handling**: Implement more robust error handling and logging mechanisms to capture and address issues during bot execution.
- **Configuration Management**: Create a configuration file to manage constants and settings, making it easier to adjust parameters without modifying the code.
- **Unit Tests**: Develop unit tests for critical functions to ensure reliability and facilitate future development.
- **Multi-Device Support**: Extend support for multiple devices running the bot simultaneously.
- **User Interface**: Develop a simple user interface to start, stop, and monitor the bot's activities.
- **Documentation**: Expand the documentation to include detailed setup instructions, usage examples, and troubleshooting tips.
- **Performance Optimization**: Optimize the bot's performance to reduce resource usage and improve execution speed.
- **Feature Expansion**: Add new features to interact with other aspects of the Space Adventure game or additional games within the Telegram app.
- **Security Enhancements**: Ensure the bot operates securely, protecting user data and preventing unauthorized access.

