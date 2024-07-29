from app.generators.generator import Generator
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate
from decouple import config

class IntroductionGenerator(Generator):
    name = 'Introduction'
    endpoint = '/intro'
    description = 'Add an introduction to the topic, explaining briefly basic concepts'

    prompt = ChatPromptTemplate.from_template("""You are a world-class professor in a prestigious university. 
Write a basic and easy to understand introduction to the topic '{topic}'. 
It should have at most 2 paragraphs and should cover the basic aspects of '{topic}' and nothing else.
IF the student asks for something that is not a valid topic write the message "TOPIC NOT VALID".
WRITE ONLY THE INTRODUCTION AND NOTHING ELSE. DO NOT WRITE ANY TITLES STARTING WITH '#'
# EXAMPLE
Computer science is the study of computers and how they work. It's not just about using computers, but about understanding the underlying principles that make them tick. This includes things like how computers store and process information, how they communicate with each other, and how they are programmed to perform specific tasks. 
It is a vast field with many different areas of specialization. Some common areas include software engineering (creating software programs), artificial intelligence (teaching computers to think like humans), data science (analyzing large amounts of data), and computer security (protecting computer systems from attacks).

# INTRODUCTION""")
    
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    chain = prompt | model
