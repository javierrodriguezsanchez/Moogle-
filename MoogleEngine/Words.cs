using Data_Base;

namespace Words;

//This class encapsulate all the relative themes to one word at the search, for better abstraction and more confortable to program


class Word
{
    //CLASS PROPETIES
    //---------------

    public string word {get; private set;}
    //the word. Nothing to explain
    public List<int> Files_That_Apear{get; private set;}
    //contains the numbers of the files that are relevant to a search and contain this word
    public double RarenestLevel {get; private set;}
    //define how rare is the word, and it gives to the word some importance inside the search
    //3 rare, 2 normal, 1 comun, 0.01 irrelevant. 0.01 to avoid errors
    //The * operator give an extra importance, 1 per * and:
    //if the word originally has 2, it has an extra 0.5 points, if it was 1 gets an extra 1.1 points, and if it has 0.01 it has an extra 2 points
    public int Total_of_Aparitions{get; private set;}
    //to avoid errors, i'll save the total of files that contains the word
    public int Priority{get; private set;}
    //the number of prints that this word have
    private bool exception;
    //for some words that are not contained in the dictionary, and for some reason I can't or I don't want to make a sugerence
    //asign 0 to the word'Rarenest, that way, the Filter.Start method will not goin to have errors
        

    //METHODS OF THE CLASS
    //--------------------

    public Word(string w,List<string> No_Words, int prints, DataBase data, bool mustBe)
    //starts the object, No_Words is UserSearch.No_operator, but I use this way because it was giving me some problems
    {    
        exception=false;
        Priority=prints;
        word=w;
        Files_That_Apear=new List<int>();
            
        if(word!="")
            getFiles(data, mustBe);
        else
        {
            Files_That_Apear=new List<int>();
                
            for (int i = 0; i < data.Files.Length; i++)
                Files_That_Apear.Add(i);
        }
        Total_of_Aparitions=Files_That_Apear.Count;
        if(exception)
        {
            RarenestLevel=0;
            return;
        }
        DeleteNoWords(data,No_Words);
        GetRarenestLevel(data,Priority);
    }

    private void getFiles(DataBase data, bool mustBe)
    //If the word apears in the dictionary, get his files
    //If it not apears, get a sugerence and replace the word whith the other one
    {
        if(mustBe && !data.words_in_files.ContainsKey(word))
        //if a word with the ^operator doesn't apear, the search is empty
        {
            exception=true;
            return;
        }

        if(!isNumber(word) && !data.words_in_files.ContainsKey(word.ToLower()))
        {
            string wordSugerence=data.Sugerence(word,2);
            //Check quickly if the most aproximated words are in the dictionary
            if(wordSugerence==word)
            {
                exception=true;
                return;
            }
            //if the most aproximated words are not contain, check every word in the database and return the smaller one
            word=wordSugerence;
        }

        string w=word;
        if(isNumber(w))
        //if a number does not apear on the dictionary, I kind of skip this word
        {
            if(!data.words_in_files.ContainsKey(w))
            {
                exception=true;
                return;
            }
        }
            
        Files_That_Apear=new List<int>(data.words_in_files[w.ToLower()]);
    }
        
    private void DeleteNoWords(DataBase data, List<string> No_Words)
    //implements the ! operator
    {
        for (int i = 0; i < No_Words.Count; i++)
        {
            if(data.words_in_files.ContainsKey(No_Words[i]))
                for(int j=0;j<data.words_in_files[No_Words[i]].Count;j++)
                    Files_That_Apear.Remove(data.words_in_files[No_Words[i]][j]);    
        }
    }

    private void GetRarenestLevel(DataBase data, int Priority)
    //asign a level of rarenest to the word
    {
        if(isNumber(word))
        {
            RarenestLevel=1+Priority;
            if(Priority>0)
                RarenestLevel+=1.1;
            return;
        }
         
        if(Total_of_Aparitions<data.Files.Length/4.0)
            RarenestLevel=3+Priority;

        else if(Total_of_Aparitions<data.Files.Length/2.0)
        {
            RarenestLevel=2+Priority;
            if(Priority>0)
                RarenestLevel+=0.5;
        }
        else if(Total_of_Aparitions<data.Files.Length*3/4.0)
        {
            RarenestLevel=1+Priority;
            if(Priority>0)
                RarenestLevel+=1.1;
        }
        else
        {
            RarenestLevel=0.01+Priority;
            if(Priority>0)
                RarenestLevel+=2.001;
        }
    }

    private static bool isNumber(string a)
    //return true if the whole entry is a number
    {
        if(a.Length==0) return false;
        for(int i=0;i<a.Length;i++)
            if(!Char.IsDigit(a[i]))
                return false;
        return true;
    }

};
