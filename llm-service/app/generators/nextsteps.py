from app.generators.generator import Generator
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate
from decouple import config

class NextStepsGenerator(Generator):
    name = 'Next Steps'
    endpoint = '/nextsteps'
    description = 'Suggestions for next topics to be explored, so that you never stop getting better.'

    prompt = ChatPromptTemplate.from_template("""You are a world-class professor in a prestigious university helping one of your students. 
Write 1 to 3 ideas of next topics to be explored after a bachelor's course in '{topic}'. Those topics need to be similar and in the order that a good university course generally presents in a syllabus.
The topics need to be related but can also include subjects of other, related, courses.
Write the title of the field and a very short 1-line description.
IF the student asks for something that is NOT a valid topic write the message "TOPIC NOT VALID".
WRITE ONLY THE TOPICS AND NOTHING ELSE.
NEVER WRITE ANY TITLES STARTING WITH '#'.
ALWAYS WRITE SOMETHING.
ALWAYS WRITE IN THE STUDENT'S LANGUAGE.
# EXAMPLE
Topic: "Computer Science"
Next Topics:
- **Artificial Intelligence:** Development of intelligent agents that can reason, learn, and act autonomously.
- **Machine Learning:** Algorithms that allow computers to learn from data and improve their performance over time.
- **Data Science:** Extraction of insights from large datasets using statistical and computational techniques.
# NEXT TOPICS
""")
    
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    chain = prompt | model
