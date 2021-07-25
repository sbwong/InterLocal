namespace UserManagement
{
    public class UserProfile {
        public int user_id;
        public string username;
        public string college;
        public string email;
        public string phone_number;
        public string country;
        public string first_name;
        public string last_name;
        public string pfp_url;
        public string year;
        public string bio;
        public int total_upvotes;

        public bool is_admin;

        public UserProfile(): this(0,"Loading","Loading","Loading","Loading","Loading","Loading","Loading","Loading","Loading", "Loading", 0, false) {

        }

        public UserProfile(int id, string usrname, string clg, string eml, string phone, string cntry, string first, string last, string pfp, string yr, string bio, int upvotes, bool is_admin) {
            this.user_id = id;
            this.username = usrname;
            this.college = clg;
            this.email = eml;
            this.phone_number = phone;
            this.country = cntry;
            this.first_name = first;
            this.last_name = last;
            this.pfp_url = pfp;
            this.year = yr;
            this.bio = bio;
            this.total_upvotes = upvotes;
            this.is_admin = is_admin;
        }

        public object export() {// only return API mandated fields
            return new {
                username = this.username,
                college = this.college,
                email = this.email,
                phone_number = this.phone_number,
                country = this.country,
                first_name = this.first_name,
                last_name = this.last_name,
                pfp_url = this.pfp_url,
                year = this.year,
                bio = this.bio,
                total_upvotes = this.total_upvotes,
                is_admin = this.is_admin
            };
        }
    }

    public class UserPrefs {
        public bool is_email_public;
        public bool is_phone_public; 
        public bool is_country_public; 
        public bool is_year_public; 
        public bool is_residential_college_public;

        public UserPrefs(bool email, bool phone, bool country, bool year, bool college) {
            is_email_public = email;
            is_phone_public = phone;
            is_country_public = country;
            is_year_public = year;
            is_residential_college_public = college;
        }

        public object export() {// only return API mandated fields
            return new {
                is_email_public = this.is_email_public,
                is_phone_public = this.is_phone_public,
                is_country_public = this.is_country_public,
                is_year_public = this.is_year_public,
                is_residential_college_public = this.is_residential_college_public
            };
        }
    }
}
