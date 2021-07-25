import requests
from bs4 import BeautifulSoup
import psycopg2

#Grab html file
url = 'https://oiss.rice.edu/student-FAQs'
page = requests.get(url)
soup = BeautifulSoup(page.content, 'html.parser')

def scrape_oiss_questions(user_id):

    question_blocks = soup.find_all('dt')

    questions = []
    for block in question_blocks:
        questions.append([user_id, block.get_text()])
    return questions

def scrape_oiss_answers():

    answer_blocks = soup.find_all('dd')

    answers = []
    for block in answer_blocks:
        answers.append(block.get_text())
    return answers[:-1]

conn = psycopg2.connect("dbname='postgres' user='comp410dbadmin@comp410-postgresql' host='comp410-postgresql.postgres.database.azure.com' password='Comp410Team4'")
with conn:
    cur = conn.cursor()
    cur.execute("DELETE FROM users WHERE username = 'OISS_Admin';")
    cur.execute("INSERT INTO users(username, college, email, country, user_status, first_name, last_name, year) VALUES ('OISS_Admin', 'Rice University', 'oiss@rice.edu', 'USA', 'admin', 'N/A', 'N/A', 'N/A') RETURNING user_id;")

    user_id = cur.fetchone()[0]

    questions = scrape_oiss_questions(user_id)
    post_ids = []

    #insert post
    for question in questions:
        cur.execute("INSERT INTO post(author_id, title) VALUES (%s, %s) RETURNING post_id;", question)
        post_ids.append(cur.fetchone()[0])
    
    answers = scrape_oiss_answers()

    #insert the content 
    for i in range(len(answers)):
        cur.execute("INSERT INTO note_content(post_id, content_body) VALUES (%s, %s);", [post_ids[i], answers[i]])