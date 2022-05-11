namespace MoogleEngine;
using Filters;
using Data_Base;
using Input;
public static class Moogle
{
    static DataBase dataBase=new DataBase("..\\Content");
    //this variable holds the common data to all posible
    static int MaxOfMemory=10;
    //the max of the historial to show
    public static SearchResult Query(string query) 
    //the program
    {    
        UserInput input = new UserInput(query,dataBase);
        //process the input of the user
        Filter search = new Filter(input,dataBase);
        //obtain the most relevant files
        SearchItem[] items = new SearchItem[search.Documents.Length];
        //creates the return
        for(int i = 0; i<search.Documents.Length;i++)
            items[i]=new SearchItem(search.DocNames[i] , search.Documents[i] , search.Snippet[i] , search.resultsValues[i]);

        return new SearchResult(items, input.query);
    }

    public static void Actualize(string query)
    //Add a new search to the historial
    {
        string newMemory=query+'\n';
        StreamReader old=new StreamReader("Memory.txt");
        newMemory+=old.ReadToEnd();
        old.Close();
        StreamWriter copy=new StreamWriter("Memory.txt");
        copy.Write(newMemory);
        copy.Close();
    }


    public static string[] Memory()
    //gets the historial
    {
        StreamReader old=new StreamReader("Memory.txt");
        List<string> retorno= new List<string>();
        for (int i = 0; i < MaxOfMemory; i++)
            retorno.Add(old.ReadLine()!);
        old.Close();
        return retorno.ToArray();
    }
}