using System;

namespace Clud.Cli.Helpers
{
    public static class ConsoleHelpers
    {
        public static void PrintLogo()
        {
            Console.Out.Write(@"
        ```````````````      
      ````````ooo````````     
     ````ooo`ooooo`ooo````    
   `````ooooo`ooo`ooooo`````  
  ```````ooo```````ooo``````` 
 `````````````````````````````
 `````````````````````````````
   `````````````@@@@@```````  
    `````````@@@@@@@@@@@```   
      @@````@@@@@@@@``@@`     
       @@@@@@@@@@@@@@@@      

       c    l    u    d

");
        }
    }
}
