using System.Text;
using Words;
using Data_Base;

namespace Input;

//this class encapsulates all things related to the user input

class UserInput
{
    //CLASS PROPETIES
    //---------------

    public string query{get;private set;}
    //search made by the user, if there is some word that not exist, that word would be changed by a sugerence
    //after the user inputs the query, this would be transformed to a certain formated, that would be easy to read by the program
    public Word [] words{get; private set;}
    //The words of the search
    public double QueryRarenest{get; private set;}
    //Holds the sum of the rarenest levels of every word from the search
    public List<string> No_operator{get; private set;}
    //keep the words with ! operator.This words must not be contain in any results
    public List<int> must_be_operator{get; private set;}
    //used to implement the ^ operator
    //save the inicial position of the words who has a ^ before
    public List<(string, string)> should_be_closer_operator{get; private set;}
    //implement the ~ operator. Save the words related
        

    //METHODS OF THE CLASS
    //--------------------

    public UserInput(string Query,DataBase data)
    //slit the query made by the user in words and get the search operators
    {
        query=Query;
        words=new Word[0];
        No_operator=new List<string>();
        must_be_operator=new List<int>();
        should_be_closer_operator=new List<(string, string)>();
            
        FormatQuery();
        string[] split=SplitQuery();
            
        if(query=="Random page" || split.Length==0)
        //when the search turns empty, the results would be random
            return;
        
        int[] Prints=GetPriority(split);

        //At this point, split only contains words formed by letter and digits        
        words=new Word[split.Length];
        for(int i=0;i<split.Length;i++)
        {
            words[i] =new Word(split[i],No_operator,Prints[i],data,must_be_operator.Contains(i));
            if(words[i].word!=split[i])
            //this happends if some word was changed for a sugestion
                query=query.Replace(split[i],words[i].word);
        }
            
        for (int i = 0; i < words.Length; i++)
            QueryRarenest+=words[i].RarenestLevel;  

        ShortWords();

    }

    private void FormatQuery()
    //Give form to the query: operatorToWord1_Word1 operatorToWord2_Word2...
    //all the operators applied to 1 word are at the left of the word whithout space
    //the ~ operator will be asigned initially to the second word
    //the only separation between words is a unique space
    {
        StringBuilder builder=new StringBuilder(query);

        char[] toIgnore=new char[]{'(',')','-','_','+','=','\\','[',']',
        '{','}',';',':','"',',','.','<','>','/','?','\n','|'};
        //this characters are pointing signs

        char[] Operators=new char[]{'!','^','*','~'};
        //this are search operators

        for (int i = 0; i < builder.Length; i++)
        {
            if((toIgnore.Contains(builder[i])))
            //replaces a sign points for space
            {
                builder.Insert(i," ");
                builder.Remove(i+1,1);
            }

            if(Operators.Contains(builder[i]) && i!=0 && !Operators.Contains(builder[i-1]) && builder[i-1]!=' ')
            {
                //separates the operators from the previous character, except the case that
                //is the first character or the previous character is space or another operator
                builder.Insert(i," ");
                i++;
            }

            if(builder[i]==' ' && (i==0 || Operators.Contains(builder[i-1]) || builder[i-1]==' '))
            //delete the extra spaces at the beginning, between operators and words and the extra ones between words
            {
                builder.Remove(i,1);
                i--;
            }
        }
            
        while(true)
        //delete all the spaces and operators at the end of the query
        {
            if(builder.Length==0)
                break;
            if(builder[builder.Length-1]==' ' || Operators.Contains(builder[builder.Length-1]))
                builder.Remove(builder.Length-1,1);
            else break;
        }

        query=builder.ToString();
    }

    private string[] SplitQuery()
    //separates in words the query, and gets the operators applied to the words
    {
        string[] split=query.Split(' ');//split the format search
            
        char[] Operators=new char[]{'!','^','*','~'};
            
        string LastCloser="";//save the previous word in case of having a ~ operator
        bool LastWasCloser=false;//says if the previous word has a ~ operator

        string correctQuery="";
        //constructs the new query

        for(int i=split.Length-1;i>=0;i--)
        //this part gets the operators aplied to the words
        {
            bool closer_sign=false;
            //say if there is a ~ operator
                
            int NoOrYes=0;
            //gets the priority between ! and ^
            //Negative would mean that ! gets the priority 
            //Positive would mean that ^ gets the priority
                
            for(int j=0;j<split[i].Length;j++)
            //At this point All the split elements contains a operators part and a word part
            //The operators part could be empty, and always is goin to be before the word part
            //The word part is not empty and only have letters and digits
            {
                if(!Operators.Contains(split[i][j]))
                //The first letter or digit marks the end of the operators aplied to the words
                {
                    if(NoOrYes<0)
                    //If there is more ! than ^, the only operator aplied would be !
                    {
                        while(Operators.Contains(split[i][0]))
                        //deleting the rest of the operators
                            split[i]=split[i].Remove(0,1);
                            
                        if(LastWasCloser)
                        //if before the word there was a closer operator, it would be ignored
                        {
                            correctQuery=correctQuery.Remove(0,1);
                            LastWasCloser=false;
                            LastCloser="";
                        }
                        No_operator.Add(split[i].ToLower());
                            
                        closer_sign=false;//if after the word there is a closer sign, it would be ignored
                        correctQuery=" !"+split[i]+correctQuery;

                        //The words with ! are no longer joined to the rest
                        string[] newSplit=new string[split.Length-1];
                        int aux=0;
                        for(int k=0;k<newSplit.Length;k++)
                        {
                                if(k==i)aux++;
                            newSplit[k]=split[k+aux];
                        }

                        for (int k = 0; k < must_be_operator.Count ; k++)
                        {
                            must_be_operator[k]--;
                        }

                        split=newSplit;
                        break;
                    }

                    if(NoOrYes>0)
                    //^ operator case
                    {
                        correctQuery=" ^"+split[i]+correctQuery;
                        must_be_operator.Add(i);
                    }

                    else
                        correctQuery=" "+split[i]+correctQuery;

                    if(LastWasCloser)
                    //when the last word was ~
                    {
                        LastWasCloser=false;
                        should_be_closer_operator.Add((split[i].Substring(j).ToLower(),LastCloser.ToLower()));
                    }

                    if(closer_sign && i!=0)
                    //~ operator case. If it is before de first word, it would be ignored
                    {
                        correctQuery=" ~"+correctQuery;
                        LastWasCloser=true;
                        LastCloser=split[i].Substring(j);
                    }

                    break;
                }

                //if the character isn't a letter or a digit, is a operator
                //the * operator is ignored in this section, because is analize in Word'class constructor
                //The rest of the operators are remove from the split element
                    
                if(split[i][j]=='!')
                {
                    split[i]=split[i].Remove(j,1);
                    j--;
                    NoOrYes--;
                }

                else if(split[i][j]=='^')
                {
                    split[i]=split[i].Remove(j,1);
                    j--;
                    NoOrYes++;
                }
                else if(split[i][j]=='~')
                {
                    split[i]=split[i].Remove(j,1);
                    j--;
                    closer_sign=true;
                }
            }
        }
        if(correctQuery.Length>0)
            query=correctQuery.Substring(1);
        else
        //in case of empty query, the item would be random
            query="Random page";

        return split;
    }

    private int[] GetPriority(string[] split)
    //gets the number of prints (*operators) of every words
    {
        int[] retorno=new int[split.Length];
        for(int i=0;i<retorno.Length;i++)
        {
            while(true)
            {
                if(split[i][0]!='*')
                    break;
                retorno[i]++;
                split[i]=split[i].Remove(0,1);
            }
        }
        return retorno;
    }

    private void ShortWords()
    //short the words of the query descending according to his respective level of rarenest
    //the words with ^operator will go first, sorted by level of rarenest desending
    {
        int pos;
        bool[] BooleanMask=new bool[must_be_operator.Count];

        //putting the words with ^ operator first
        for(int i=0;i<must_be_operator.Count;i++)
        {
            pos=0;
            for (int j = 1; j < must_be_operator.Count; j++)
            {
                if(!BooleanMask[j] && words[must_be_operator[j]].RarenestLevel>words[must_be_operator[pos]].RarenestLevel)
                    pos=j;
            }
            if (BooleanMask.Length>0)
            {
                BooleanMask[pos]=true;
            }

            Word c=words[must_be_operator[pos]];
            words[must_be_operator[pos]]=words[i];
            words[i]=c;
        }

        for(int i=must_be_operator.Count;i<words.Length;i++)
        {
            pos=i;
            for (int j = i; j < words.Length; j++)
                if(words[pos].RarenestLevel<words[j].RarenestLevel)
                    pos=j;
                
            if(pos!=i)
            {    
                Word c=words[pos];
                words[pos]=words[i];
                words[i]=c;
            }
        }
    }
};


