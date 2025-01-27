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
