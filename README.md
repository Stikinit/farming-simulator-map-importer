# farming-simulator-map-importer

<div align="center">

[![MIT License][license-shield]][license-url]
[![Stars][stars-shield]][stars-url]\
[![Unity][unity-shield]][unity-url]
[![C#][csharp-shield]][csharp-url]
[![Visual Studio][vs-shield]][vs-url]

<br/>
</div>

<div align="center">

  <br/>
  <h1>Farming Simulator Map Importer</h1>
  
  This project was created as part of an internship for CNH Industrial. By using the Unity Engine, a tool was developed that could automate the importing process   of a <i>Farming Simulator</i> map into a <b>HDRP Unity Project</b>.
  
</div>

## Features
<ul>
  <li><b>Unity</b>'s Editor GUI classes are used to design the complete tool's UI.</li>
  <li>The tool's main feature correctly assigning textures from the game's directory to the scene's empty meshes extracted using <b>GIANTS Editor</b>, the game's modding software.</li>
  <li>Two shaders were developed using <b>ShaderGraph</b>:
    <ol>
      <li><b>FS_Lit</b> handles the textures from the game and correctly applies them to the object.</li>
      <li><b>FS_terrain</b> creates a complete texture from a set of <b>masks</b> and <b>tiles</b> that is later applied to the terrain.</li>
    </ol>
  </li>
  <li>Farming Simulator's trees are not compatible with <b>Unity</b>. This problem was handled by creating personal prefabs of different types of trees and then substituting the corrupted trees with the new models, while paying attention to their species.</li>
</ul>

## UI
<img width="70%" src="./images/final_tool"/>

## Results
<img width="70%" src="./images/us_map"/>

<img width="70%" src="./images/alpine_map"/>

## Usage
As of now, i'll not be showcasing how to use the Unity Tool. This repo is beign utilized as an archive for the project.<br>
In the future, however, i could create a short video or document detailing the whole process.

[unity-shield]: https://img.shields.io/badge/Unity-000000?logo=unity&logoColor=white
[unity-url]: https://unity.com/
[csharp-shield]: https://img.shields.io/badge/C%23-%23239120.svg?logo=c-sharp&logoColor=white
[csharp-url]: https://docs.microsoft.com/en-us/dotnet/csharp/
[vs-shield]: https://img.shields.io/badge/Visual%20Studio-5C2D91.svg?logo=visual-studio-code&logoColor=white
[vs-url]: https://visualstudio.microsoft.com/
[license-shield]: https://img.shields.io/github/license/iss2022-BCR/WasteService
[license-url]: https://github.com/Stikinit/farming-simulator-map-importer/blob/main/LICENSE
[stars-shield]: https://custom-icon-badges.herokuapp.com/github/stars/Stikinit/farming-simulator-map-importer?logo=star&logoColor=yellow
[stars-url]: https://github.com/Stikinit/farming-simulator-map-importer/stargazers
