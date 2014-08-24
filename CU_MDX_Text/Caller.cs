using System;
using System.Collections.Generic;
using System.Text;

namespace CUnit
{
  class Caller
  {
    public Caller()
    {
      for(int i = 0; i < Numbers.Length; ++i)
      {
        Numbers[i] = false;
      }

      // to do: open a file
    }

    public void Call()
    {
      // check for a completed game
      Finished = true;
      for (int i = 0; i < Numbers.Length; ++i)
      {
        if (!Numbers[i])
        {
          Finished = false;
          break;
        }
      }
      if (Finished)
      {
        return;
      }

      // get the next random number
      int index;
      do
      {
        index = _random.Next(Numbers.Length);
      }
      while (Numbers[index]);

      // set the new game state
      Numbers[index] = true;
      LastNumber = index + 1;

      // to do: write new number to file and save
    }

    public bool Finished = false;
    public int LastNumber = 0; // 0 means no number yet called
    public bool[] Numbers = new bool[75];
    Random _random = new Random();
  }
}
