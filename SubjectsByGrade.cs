namespace StudyResource
{
    public class SubjectsByGrade
    {
        public static readonly Dictionary<int, List<(string Name, int SubjectId)>> GradeSubjects = new Dictionary<int, List<(string Name, int SubjectId)>>
        {
            { 9 , new List<(string, int)>
                {
                
                }
            },
            { 10, new List<(string, int)>
                {
                    ("Toán", 1),
                    ("Ngữ văn", 2),
                    ("Vật lý", 3),
                    ("Hóa học", 4),
                    ("Sinh học", 5),
                    ("Lịch sử", 6),
                    ("Địa lý", 7),
                    ("Tiếng Anh", 8),
                    ("Giáo dục công dân", 9),
                    ("Tin học", 10),
                }
            },
            { 11, new List<(string, int)>
                {
                    ("Toán", 1),
                    ("Ngữ văn", 2),
                    ("Vật lý", 3),
                    ("Hóa học", 4),
                    ("Sinh học", 5),
                    ("Lịch sử", 6),
                    ("Địa lý", 7),
                    ("Tiếng Anh", 8),
                    ("Giáo dục công dân", 9),
                    ("Tin học", 10),
                }
            },
            { 12, new List<(string, int)>
                {
                    ("Toán", 1),
                    ("Ngữ văn", 2),
                    ("Vật lý", 3),
                    ("Hóa học", 4),
                    ("Sinh học", 5),
                    ("Lịch sử", 6),
                    ("Địa lý", 7),
                    ("Tiếng Anh", 8),
                    ("Giáo dục công dân", 9),
                    ("Tin học", 10),
                }
            },
        };
    }
}
