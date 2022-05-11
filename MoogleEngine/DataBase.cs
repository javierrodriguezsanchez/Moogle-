using System.Text ;

namespace Data_Base ;



class DataBase
//This class contains all the comun data to all posible search and some process related to the database
{
    //CLASS PROPETIES
    //---------------

    public string [] Files { get ; private set ; }
    //The files would be save here
    public string [] FilesNames { get ; private set ; }
    //These are the names of the files
    public Dictionary < string , List < int > > words_in_files { get ; private set ; }
    //this dictionary asociates each word from every file with the index of the files who contains the word
    public Dictionary < string , List < int > > [] Words_Positions_In_File { get ; private set ; }
    //each element of the array represents a document
    //each collection of keys of the elements of the array represent each word of the document
    //The lists represent the position list of every word in every file 
    public int [] Max_Repetition_in_File { get ; private set ; }
    //get the number of repetitions of the most repeated word of every file
    static char[] characters = new char [] { 'h' , 'a' , 'e' , 'i' , 'o' , 'u' , 'l' , 'r' , 's' , 'n' , 'm' , 'd' , 'p' , 'c' , 't' , 'b' , 'f' , 'v' , 'g' , 'j' , 'ñ' , 'á', 'é' , 'í' , 'ó' , 'ú' ,'y' , 'z' , 'x' , 'q' , 'k' , 'w'} ;
    //all the characters



    //METHODS OF THE CLASS
    //--------------------

    public DataBase ( string direction )
    //fills up the databases with the .txt contained at the direction in the string parameter
    {
        Files = Directory.GetFiles( direction , "*.txt" ) ;
        
        Max_Repetition_in_File = new int [ Files.Length ] ;
        Words_Positions_In_File = new Dictionary< string , List < int > > [ Files.Length ] ;
        words_in_files = new Dictionary < string , List < int > > ( ) ;
        FilesNames = new string [ Files.Length ] ;


        char [] toIgnore = new char [] { '(' , ')' , '-' , '_' , '+' , '=' , '\\' , '[' , ']' , '{' , '}',
        ';' , ':' , '"' , ',' , '.' , '<' , '>' , '/' , '?' , ' ' , '\n' , '|' , '!' , '~' , '*' , '^' , '\0' } ;
        //caracters list to ignore when I pass the documents to the dictionary
            
        StreamReader b ;

        for ( int i = 0 ; i < Files.Length ; i++ )
        {
            b = new StreamReader ( Files [ i ] ) ;
            FilesNames [ i ] = Files [ i ] . Substring ( direction.Length + 1 ) ;
            FilesNames [ i ] = FilesNames [ i ] . Remove ( FilesNames [ i ] . Length - 4 ) ;            
            Files [ i ] = b . ReadToEnd ( ) ;
            Words_Positions_In_File [ i ] = new Dictionary < string , List < int > > ( ) ;
            
            int max = 1 ;
            //save the repetitions of the most repeated word
            string [] split = Files [ i ] . ToLower ( ) . Split ( toIgnore ) ;
            //gets the words of the i-est file

            for ( int j  =  0 ; j < split . Length ; j ++ )
            {        
                if ( split [ j ] . Length == 0 ) continue ;
                //errors may happend

                if( words_in_files . ContainsKey ( split [ j ] ) )
                //if the word already apear
                {
                    if( ! words_in_files [ split [ j ] ] . Contains ( i ) )
                    //if it doesn't have apeared in the file yet
                        words_in_files [ split [ j ] ] . Add ( i ) ;
                    
                }

                else
                {
                    words_in_files . Add ( split [ j ] , new List < int > { i } ) ;
                    //if it haven't apear, add the word
                }

                if ( ! Words_Positions_In_File [ i ] . ContainsKey ( split [ j ] ) )
                    Words_Positions_In_File [ i ] . Add ( split [ j ] , new List < int > ( ) ) ;    
                Words_Positions_In_File [ i ] [ split [ j ] ] . Add ( j ) ;
                if(max < Words_Positions_In_File [ i ] [ split [ j ] ] . Count )
                    max = Words_Positions_In_File [ i ]  [ split [ j ] ] . Count ;
                
            }

            Max_Repetition_in_File [ i ] = max ;
        }
    }    
            
    public string Sugerence ( string target , int max )
    //check if a word whith a Levenstein distance (1 -> max) of the target is contained in the files and return that word
    //if there isn't words at that distance, return the same word
    //the n-est distance is some of the words at distance 1 from the distance n-1 words
    {
        string b;
            
        List< List < string > > Distances = new List < List < string > > ( ) ;
        //The list at the position n is the list of the words at distance n from the target word
        Distances . Add ( new List < string > ( ) ) ;
        Distances [ 0 ] . Add ( target ) ;
            
        HashSet < string > Obtained = new HashSet < string > ( ) { target } ;
        //this is the collection of the words checked

        for( int i = 0 ; i < max ; i ++ )
        //every iteration of the cicle check a distance of the word
        {
            Distances . Add ( new List < string > ( ) ) ;
            b = GetNextDistance ( Distances , i , Obtained ) ;
            if( b != "" )
                return b ;
        }
            
        return target ;
    }

    private string GetNextDistance ( List < List < string > > distances, int pos, HashSet < string > Obtained )
    //gets all the words at distance 1 of the words in the position pos of the distance'list except the words in the Obtained collection
    //The distance 1 of a word is obtained through: adding a letter, changing a letter, removing a letter
    {
        for( int i = 0 ; i < distances [ pos ] . Count ; i ++ )
        {
            StringBuilder builder = new StringBuilder ( distances [ pos ] [ i ] ) ;
            
            char OldChar ;
            //save the character that will be deleted to put it again when the process is finished

            for ( int c = 0 ; c < characters . Length ; c ++ )
            {
                for  ( int p  =  0 ; p < builder . Length ; p ++ )
                //select every position and every character
                {
                    //adding a letter in the position i
                    OldChar = builder [ p ] ;
                    builder . Insert ( p , characters [ c ] ) ;
                    if ( Obtained . Contains ( builder . ToString ( ) ) )
                    {
                        builder . Remove ( p , 1 ) ;
                        continue ;
                    }
                        
                    distances [ pos + 1 ] . Add ( builder . ToString ( ) ) ;
                    Obtained . Add ( builder . ToString ( ) ) ;
                    if ( words_in_files . ContainsKey ( builder . ToString ( ) ) )
                        return builder . ToString ( ) ;
                        
                    //changing a letter at the position i througth deletting the letter in the position i+1 and preserv the letter added in the previous step
                    builder . Remove ( p + 1 , 1 ) ;
                        
                    if ( Obtained . Contains ( builder . ToString ( ) ) )
                    {
                        builder . Insert ( p , OldChar ) ;
                        builder . Remove ( p + 1 , 1 ) ;
                        continue ;
                    }
                        
                    distances [ pos + 1 ] . Add ( builder . ToString ( ) ) ;
                    Obtained . Add ( builder . ToString ( ) ) ;
                    if ( words_in_files . ContainsKey ( builder . ToString ( ) ) )
                        return builder . ToString ( ) ;
                        
                    builder.Insert ( p , OldChar ) ;
                    builder.Remove ( p + 1 , 1 ) ;
                }

                builder . Append ( characters [ c ] ) ;
                if ( Obtained . Contains ( builder . ToString ( ) ) )
                {
                    builder . Remove ( builder . Length - 1 , 1 ) ;
                    continue ;
                }
                    
                distances [ pos + 1 ] . Add ( builder . ToString ( ) ) ;
                Obtained . Add ( builder . ToString ( ) ) ;
                if ( words_in_files . ContainsKey ( builder . ToString ( ) ) )
                    return builder . ToString ( ) ;
                    
                builder . Remove ( builder . Length - 1 , 1 ) ;
            }

            //removing a letter from every position
            for (int p  =  0 ; p < builder.Length ; p ++ )
            {
                OldChar = builder [ p ] ;
                builder . Remove ( p , 1 ) ;
                    
                if( Obtained . Contains ( builder . ToString ( ) ) )
                {
                    builder . Insert ( p , OldChar ) ;
                    continue ;
                }
                    
                distances [ pos + 1 ] . Add ( builder . ToString ( ) ) ;
                Obtained . Add ( builder . ToString ( ) ) ;
                if( words_in_files . ContainsKey ( builder . ToString ( ) ) )
                    return builder . ToString ( ) ;

                builder . Insert ( p , OldChar ) ; 
            }
        }
        return "" ;
    }

    
    public int GetMinPosDistance ( int file , string word1 , string word2 )
    //returns how close are the words in a file
    {

        if( ! Words_Positions_In_File [ file ] . ContainsKey ( word1 ) || !Words_Positions_In_File [ file ] . ContainsKey ( word2 ) )
            return int . MaxValue ;

        int minDistance = int . MaxValue ;

        if ( word1  ==  word2 )
        //the two words could be equals, thats why I make this case diferent
        {
            for  ( int i  =  0 ; i < Words_Positions_In_File [ file ] [ word1 ] . Count-1 ; i ++ )
            {
                if ( Math . Abs ( Words_Positions_In_File [ file ] [ word1 ] [ i ] - Words_Positions_In_File [ file ] [ word1 ] [ i + 1 ] ) < minDistance )
                    minDistance = Math . Abs ( Words_Positions_In_File [ file ] [ word1 ] [ i ] - Words_Positions_In_File [ file ] [ word1 ] [ i + 1 ] ) ;
                
                if ( minDistance == 1 ) break ;

            }
            return minDistance ;
        }

        int j = 0 ;
        //the index for the second word
            
        for  ( int i  =  0 ; i < Words_Positions_In_File [ file ] [ word1 ] . Count ; i ++ )
        {
            if ( j == Words_Positions_In_File [ file] [ word2 ] . Count )
                break ;
            if ( Math . Abs ( Words_Positions_In_File [ file ] [ word1 ] [ i ] - Words_Positions_In_File [ file ] [ word2 ] [ j ] ) < minDistance )
                minDistance = Math . Abs ( Words_Positions_In_File [ file ] [ word1 ] [ i ] - Words_Positions_In_File [ file ] [ word2 ] [ j ] ) ;

            if ( Words_Positions_In_File [ file ] [ word1] [ i] > Words_Positions_In_File [ file ] [ word1 ] [ j ] )
            {
                j ++ ;
                i -- ;
            }
        }

        return minDistance ;
    }

}

    