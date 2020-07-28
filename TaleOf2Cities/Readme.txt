Approach
We maintain static dictionary to record the character and word counts and convert to list for sorting. A common method is used to sort the dictionary by key or value.
WritetoFIle method will do the heavy lifting of writing all the sortedlist for each scenario to the file. that way writing to file is a sequential operation.

We read each line and fork it into 4 parallel tasks which will record the character and word counts. At any moment only one thread will be updating the individual static collection
and hence this is threadsafe.Over all operations completes in < 2 seconds.This way we can handle any bigfile with out worrying baout the memory capacity.


Pre Requisite:
1) This require dotnet core version 3.1 to run

Configuration:

Please update the below values in talesettings.json

{
  "TaleSettings": {
    "inputFileName": "A Tale of Two Cities - Charles Dickens.txt",
    "searchKeyword": "apartment",
    "KthTopOrderWordPosition": 5,
    "OutputFileNamePrefix": "ProcessedFile"
  }

}

Input folder will have the file to be parsed
Output FOlder will have the processed summary of the file. Search for SC1 to SC5 keyword to see each scenario output.

Exceptions:
Any exceptions will be logged in the console.

Tests:

It require bit of effort to Moq the file reader class and hence tested with Manual integration tests.

SC1: Character count 
SC2:
