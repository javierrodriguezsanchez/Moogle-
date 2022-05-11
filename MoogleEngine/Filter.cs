namespace Filters;

using System;
using Data_Base;
using Words;
using Input;

//this class obtains the documents most relevant to a UserInput from a DataBase 

class Filter
{
    //CLASS PROPETIES
    //---------------

    static int MaxOfResults=10;
    //give the max number of results wanted   
    private List<int> Files_in_The_Filter;
    //Holds the significant files positions in the data base
    private List<List<int>> Words_of_Files_in_the_Filter;
    //Each i-element is a list of search's words contained by the file in the i-position on Files_in_The_Filter
    public float[] resultsValues{get; private set;}
    //the values of the result
    public string[] Documents{get; private set;}
    //holds the most relevant documents
    public string[] DocNames{get; private set;}
    //holds the name of the documents
    public string[] Snippet{get; private set;}
    //holds a relevant part of the documents



    //MAIN OBJECT METHODS
    //-------------------

    public Filter( UserInput search , DataBase database)
    //starts the object
    {
        Files_in_The_Filter = new List < int > ( ) ;
        Words_of_Files_in_the_Filter = new List < List < int > > ( ) ;
        resultsValues = new float [ 0 ] ;
        
        if( search.query == "Random page" || search.words.Length==0)
        //return the user random pages if there is nothing to search
        {
            Randomize( search.No_operator.ToArray( ) , database, search.No_operator) ;
            Documents=new string[Files_in_The_Filter.Count];
            Snippet=new string[Documents.Length];    
            DocNames=new string[Documents.Length];
            for (int i = 0; i < Documents.Length; i++)
            {
                DocNames[i]=database.FilesNames[Files_in_The_Filter[i]];
                Documents[i]=database.Files[Files_in_The_Filter[i]];
                Snippet[i]=Documents[i].Substring(Math.Min(Documents[i].Length,20));
            }
            return ;
        }

        ApplyFilter( search ) ;

        List<double> values = new List < double > ( 0 ) ;
        //give values to the files in the filter
        for ( int i = 0 ; i < Files_in_The_Filter.Count ; i++ )
            values.Add( TF_IDF ( Files_in_The_Filter[i] , i , database, search.words) * CloserBonus(Files_in_The_Filter[i], database, search.should_be_closer_operator) ) ;//Apply TF_IDF to each file to analize

        ShortAndReduce ( values ) ;

        resultsValues = new float [ Files_in_The_Filter.Count ] ;

        for ( int i = 0 ; i < Files_in_The_Filter.Count ; i++ )
        {
            resultsValues [ i ] = ( float ) values [ i ] ;   
        }
        //delete the rest of them

        Documents=new string[Files_in_The_Filter.Count];
        DocNames=new string[Files_in_The_Filter.Count];

        for (int i = 0; i < Documents.Length; i++)
        {
            DocNames[i]=database.FilesNames[Files_in_The_Filter[i]];
            Documents[i]=database.Files[Files_in_The_Filter[i]];
        }


        Snippet=new string[Documents.Length];
        for (int i = 0; i < Snippet.Length; i++)
        {
            string[] Paragraphs=Documents[i].ToLower().Split('\n');
            Snippet[i]=SnippetText(Paragraphs, search.words);
        }
    }
    
    private void ApplyFilter(UserInput search)
    //Start filtring the files.
    //The ones who saves in files_in_the_filter variable are those that present the next 2 propeties:
    //1- the sum of the rarenest level of the words from the query contained in that file, is bigger that the other words from the query
    //2-it presents all the ^operators words
    {
        if(search.words.Length==0)
            return;

        int file; 
        
        List<int> File_Search_Words=new List<int>(0);

        double AproxValue,UnincludedRarenest,OldWordsRarenest=0;
        //AproxValue takes a file rarenest acording to the words of the query that contains
        //UnincludedRarenest y OldWordsRarenest serve to optimize, to know the rarenest that the file doesn't containe
        
        bool no_must_be_operator= search.must_be_operator.Count==0;
      
        for(int i=0; i<(no_must_be_operator?search.words.Length:1) ;i++)
        //the cicle goes througth each word unless there is ^words, in this case its only analize the files that contains the first element
        {
            for(int j=0;j<search.words[i].Files_That_Apear.Count;)
            //this cicle goes througth the files that contains the i-st word
            //When a file is analized, its deleted, because the program will not use them any more 
            {
                UnincludedRarenest=OldWordsRarenest;//UnincludedRarenest now has a value equals the sum of the level of rarenest of the words previous to the i-st word 
                
                if(search.QueryRarenest-UnincludedRarenest<UnincludedRarenest)
                //if the posible rarenest to include is less that the rarenest excluded, there isn't point to keep goin
                    break;

                file=search.words[i].Files_That_Apear[j];
                File_Search_Words.Add(i);
                AproxValue=search.words[i].RarenestLevel;
                bool HaveAllMustBeWords=true;
            
                for(int k=i+1; k<search.words.Length && (search.words[0].RarenestLevel==0.01 || search.words[k].RarenestLevel!=0.01) ;k++)
                //this cicle looks what other words of the search the file contains
                { 
                    if(search.words[k].Files_That_Apear.Remove(search.words[i].Files_That_Apear[j]))
                    //Files_That_Apear.Remove is a bool method, and return true if the number j was between the files of the word[k]
                    //the file is deleted because we not used again and to avioid checking a file already checked
                    {
                        File_Search_Words.Add(k);
                        AproxValue+=search.words[k].RarenestLevel;
                    }
                    else if(k<search.must_be_operator.Count)
                    //if the file not contain a word that must be there, its also pointless keep searching
                    {
                        HaveAllMustBeWords=false;
                        break;
                    }

                    else
                    {
                        UnincludedRarenest+=search.words[k].RarenestLevel;//holds the non-saved rarenest
                        if(search.QueryRarenest-UnincludedRarenest<UnincludedRarenest)
                            break;//another pointless situation to keep analizing
                    }
                }
                
                if(HaveAllMustBeWords && AproxValue>=search.QueryRarenest-AproxValue)
                //If the file have all the words with ^operator, and the rarenes included is bigger that the not included, this file is relevant to the search
                {
                    Files_in_The_Filter.Add(file);
                    Words_of_Files_in_the_Filter.Add(File_Search_Words);
                }
                
                search.words[i].Files_That_Apear.Remove(search.words[i].Files_That_Apear[0]);
                //The program will not use this anymore
                File_Search_Words=new List<int>(0);
            }
            
            OldWordsRarenest+=search.words[i].RarenestLevel;
            //Holds the rarenest for next iterations
        }
    }



    //AUXILIAR AND PUBLIC METHODS
    //---------------------------
    private double TF_IDF(int f, int pos, DataBase database, Word[] WordsOfTheSearch)
    //return the TF-IDF value of the document no.f
    {
        double TFIDF=0;
        for(int i=0; i<Words_of_Files_in_the_Filter[pos].Count;i++)
        {
            string w=WordsOfTheSearch[Words_of_Files_in_the_Filter[pos][i]].word.ToLower();
            
            TFIDF += (0.5+0.5*database.Words_Positions_In_File[f].Count*1.0/database.Max_Repetition_in_File[f])
            * Math.Log((database.Files.Length+1.0)/(database.words_in_files[w].Count+1.0))*(WordsOfTheSearch[i].Priority+1);    
        }
        return TFIDF;
    }
    
    private double CloserBonus(int file, DataBase database, List<(string,string)> pairs)
    //applys the ~ operator
    {
        if(pairs.Count==0)
            return 1;
        
        double bonus=1;
        
        for (int i = 0; i < pairs.Count; i++)
        {
            if(!database.Words_Positions_In_File[file].ContainsKey(pairs[i].Item1) || !database.Words_Positions_In_File[file].ContainsKey(pairs[i].Item2))
                continue;
            bonus*=1+1/Math.Sqrt(database.GetMinPosDistance(file,pairs[i].Item1,pairs[i].Item2));
        }
        return bonus;
    }

    private void ShortAndReduce(List<double> values)
    //Short be value and reduce to MaxOfResults number or less the Files in the filter
    {
        int pos;
        for(int i=0;i<MaxOfResults;i++)//Put the biggers files at the beggining
        {
            pos=i;
            for (int j = i+1; j < Files_in_The_Filter.Count; j++)
                if(values[j]>values[pos])
                    pos=j;
            if(i!=pos)
            {
                int aux=Files_in_The_Filter[i];
                Files_in_The_Filter[i]=Files_in_The_Filter[pos];
                Files_in_The_Filter[pos]=aux;
                
                double aux2=values[i];
                values[i]=values[pos];
                values[pos]=aux2;
            }
        }

        if(Files_in_The_Filter.Count>MaxOfResults)
            Files_in_The_Filter.RemoveRange(MaxOfResults,Files_in_The_Filter.Count - MaxOfResults);
    }
    
    private void Randomize(string[] exception, DataBase database, List<string> No_Words)
    //give random results to the user
    {
        Random random=new Random();
        resultsValues=new float[1];
        
        Files_in_The_Filter.Clear();

        Word w=new Word("",No_Words,0,database,false);

        for (int i = 0; i < 1; i++)
        {
            int a=random.Next(0,w.Files_That_Apear.Count);

            Files_in_The_Filter.Add(w.Files_That_Apear[a]);
                
            w.Files_That_Apear.Remove(a);

            resultsValues[i]=0;
        }
    }

    private string SnippetText(string[] Paragraphs, Word[] Words_Of_The_Search)
    //return a colection of paragraphs in a single string that contains all the words of the query contained by the document
    {
        string retorno="";
        bool[] Mask=new bool[Words_Of_The_Search.Length];
        char[] toIgnore=new char[]{'(',')','-','_','+','=','\\','[',']',
        '{','}',';',':','"',',','.','<','>','/','?',' ','\n','|','!','~','*','^','\0'};
        for (int i = 0; i < Paragraphs.Length; i++)
        {
            string[] split=Paragraphs[i].Split(toIgnore);
            bool added=false;
            for (int j = 0; j < Words_Of_The_Search.Length; j++)
            {
                if (!Mask[j] && split.Contains(Words_Of_The_Search[j].word.ToLower()))
                {
                    Mask[j]=true;
                    added=true;
                }
            }
            if(added)
            {    
                retorno+=Paragraphs[i]+"(...)";
                if(AllMaskIsTrue(Mask))
                    break;
            }
        }
        return retorno.Remove(retorno.Length-5);
    }

    private bool AllMaskIsTrue(bool[] Mask)
    //return true if every element in Mask is true
    {
        for (int i = 0; i < Mask.Length; i++)
            if(!Mask[i])return false;
        return true;
    }
}