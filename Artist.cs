namespace DualScreenDemo
{
    public class Artist
    {
        
        public string Name { get; set; }
        
        
        public string Phonetic { get; set; }
        
        
        public string Category { get; set; }
        
        
        public int Strokes { get; set; }

        
        public Artist(string name, string phonetic, string category, int strokes)
        {
            Name = name;
            Phonetic = phonetic;
            Category = category;
            Strokes = strokes;
        }

        
        public override string ToString()
        {
            return $"Name: {Name}, Phonetic: {Phonetic}, Category: {Category}, Strokes: {Strokes}";
        }
    }
}