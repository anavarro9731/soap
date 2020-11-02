Param(
    [Parameter(Mandatory = $true)]
    [string] $componentName
   )

   function MoveToReactComponentsRootDirectory {
      Set-Location $PSScriptRoot
   }

   function Build {
       
       function WriteIndexFile
       {
           @"
<!DOCTYPE html>
<html lang='en'>
   <head>
      <meta charset='utf-8' />
      <!-- Consider mobile layout -->
      <meta name='viewport' content='width=device-width, initial-scale=1' />
      <title>Playground</title>
   </head>
   <body>
      <div id='content'></div>
   </body>
   </html>
   <script src='$appPath/playground.js'></script>
"@ | out-file -encoding utf8 ./index.html
       }

       WriteIndexFile
       
       npx parcel ./index.html
   }
   
   function CleanComponentPath {

      Param([string] $componentName)
      
      $path = "./src/$componentName"
      
      if (-not (Test-Path "$PSScriptRoot/$path")) {
         throw "Directory $path invalid"
      }

      return $path
   }

function Main {

    try {

      $initialDirectory = $PWD

      $appPath = CleanComponentPath $componentName
      
      MoveToReactComponentsRootDirectory

      Build

    }
    finally {

        Set-Location $initialDirectory 
   }
}

Main