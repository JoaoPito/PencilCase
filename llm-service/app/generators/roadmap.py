from app.generators.generator import Generator
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate
from decouple import config

class RoadmapGenerator(Generator):
    name = 'Roadmap'
    endpoint = '/roadmap' 
    description = 'Adds clear and easy-to-follow topics, starting from the basics.'

    prompt = ChatPromptTemplate.from_template("""You are a world-class professor in a prestigious university. Write an easy to follow Roadmap for a study guide on '{topic}'. 
It should have 3-4 main topics that need to be studied, those topics need to be similar to what's in a syllabus of a university bachelor's course, from basic to advanced.
Those topics can have 2-5 subtopics, ordered the same way.
IF the student asks for something that is not a valid topic write the message "TOPIC NOT VALID".
WRITE ONLY THE ROADMAP AND NOTHING ELSE, EVEN IF THE STUDENT ASKS YOU TO. 
NEVER WRITE ANY TITLES STARTING WITH '#'. 
ALWAYS WRITE SOMETHING. 
ALWAYS WRITE IN THE STUDENT'S LANGUAGE.
# EXAMPLE
1. **Topic 1:** 1-line description of the topic
    1.1. **Subtopic 1.1:** 1-line Description
    1.2. **Subtopic 1.2:** 1-line Description
2. **Topic 2:** 1-line description of the topic
    2.1. **Subtopic 2.1:** 1-line Description
    2.2. **Subtopic 2.2:** 1-line Description
    2.3. **Subtopic 2.3:** 1-line Description
    2.4. **Subtopic 2.4:** 1-line Description
# ROADMAP""")
    
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    chain = prompt | model
