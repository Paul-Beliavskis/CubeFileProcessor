# CubeFileProcessor

## Task overview
Your task is to process a list of text files uploaded into an Azure file share* by users on daily basis**. You'd be required to run a pattern*** 
for each text line of the file and if any matched then the file needs to be moved into a separate output folder or else it deleted from its original location.

## Getting started
1. Clone the repo 
2. Update the local.settings.json file which has the following values:</br></br>
**sourceDirName** - The directory that will contain the files that need to be parsed (if it doesn't exist it will be created).</br>
**saCubeShareName** - The name of the file share.</br>
**outputDirName** - Directory to output the files that match the Regex.</br>
**saConnectStr** - The connection string to the storage account.</br>
**regexToMatch** - The regex that will be used to check each line in each file. </br>

3. Run the project
## Thought process
Alothought I have created two functions within the same function app in a production setting I would have two separate apps and move the 
storage account connection logic into a Nuget package hosted on Azure DevOps Artifacts and then use this package in each of the function apps.
The reason for this is that the timer trigger is not supported on a consumption based plan and so it would be much more cost efficient to have the timer
on a low teir plan and have the consumption plan for the file processing which will be a weekly task.
</br>
We couldn't do this all in one function because of the possible Azure function timeouts which is why Microsoft recommend creating functions as short lived as possible.
Hence an instance of a function only processes one file at a time keeping execution time to a minimum.
