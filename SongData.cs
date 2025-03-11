using System;

namespace DualScreenDemo
{
    public class SongData
    {
        public string SongNumber { get; set; } 
        public string Category { get; set; }
        public string Song { get; set; }
        public double Plays { get; set; }
        public string ArtistA { get; set; }
        public string ArtistB { get; set; }
        public string ArtistACategory { get; set; }
        public string ArtistBCategory { get; set; }
        public DateTime AddedTime { get; set; } 
        public string SongFilePathHost1 { get; set; } 
        public string SongFilePathHost2 { get; set; }
        public string PhoneticNotation { get; set; } 
        public string PinyinNotation { get; set; } 
        public string ArtistAPhonetic { get; set; } 
        public string ArtistBPhonetic { get; set; } 
        public string ArtistASimplified { get; set; } 
        public string ArtistBSimplified { get; set; } 
        public string SongSimplified { get; set; } 
        public string SongGenre { get; set; }
        public string ArtistAPinyin { get; set; } 
        public string ArtistBPinyin { get; set; } 
        public int HumanVoice { get; set; } 

        
        public SongData(string songNumber, string category, string song, double plays, string artistA, string artistB, string artistACategory, string artistBCategory, DateTime addedTime, string songFilePathHost1, string songFilePathHost2, string phoneticNotation, string pinyinNotation, string artistAPhonetic, string artistBPhonetic, string artistASimplified, string artistBSimplified, string songSimplified, string songGenre, string artistAPinyin, string artistBPinyin, int humanVoice)
        {
            SongNumber = songNumber;
            Category = category;
            Song = song;
            Plays = plays;
            ArtistA = artistA;
            ArtistB = artistB;
            ArtistACategory = artistACategory;
            ArtistBCategory = artistBCategory;
            AddedTime = addedTime;
            SongFilePathHost1 = songFilePathHost1; 
            SongFilePathHost2 = songFilePathHost2;
            PhoneticNotation = phoneticNotation; 
            PinyinNotation = pinyinNotation;
            ArtistAPhonetic = artistAPhonetic;
            ArtistBPhonetic = artistBPhonetic;
            ArtistASimplified = artistASimplified;
            ArtistBSimplified = artistBSimplified;
            SongSimplified = songSimplified;
            SongGenre = songGenre;
            ArtistAPinyin = artistAPinyin;
            ArtistBPinyin = artistBPinyin;
            HumanVoice = humanVoice; 
        }

        public override string ToString()
        {
            
            return !string.IsNullOrWhiteSpace(ArtistB)
                ? String.Format("{0} - {1} - {2}", ArtistA, ArtistB, Song)
                : String.Format("{0} - {1}", ArtistA, Song);
        }
    }
}