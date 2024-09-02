from app.generators.generator import Generator
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate
from decouple import config

class ApplicationsGenerator(Generator):
    name = 'Application Suggestions'
    endpoint = '/applications'
    description = 'Suggests 3-5 real-world applications for the given topic.'

    prompt = ChatPromptTemplate.from_template("""You are a world-class professor in a prestigious university helping one of your students. 
Write 3 to 5 insightful application ideas for the topic '{topic}'. Those ideas should be realistic and possible to be executed in the real world.
If research is required to execute any of these ideas, write which topics are needed.
Each idea SHOULD BE at most 1 paragraph long.
IF the student asks for something that is NOT a valid topic write the message "TOPIC NOT VALID".
WRITE ONLY THE INTRODUCTION AND NOTHING ELSE.
NEVER WRITE ANY TITLES STARTING WITH '#'.
ALWAYS WRITE SOMETHING.
ALWAYS WRITE IN THE STUDENT'S LANGUAGE.
# EXAMPLE
Topic: "Optics"
1. **Smart Glasses for Vision Correction and AR:** Develop glasses that adjust focus automatically and integrate augmented reality for enhanced vision and functionality, requiring research in *adaptive optics* and *AR technologies*.
2. **Non-Invasive Glucose Monitoring:** Create wearable optical sensors to track blood glucose levels through the skin, needing exploration in *near-infrared spectroscopy* and *biocompatible sensors*.
3. **High-Speed Optical Communication:** Innovate fiber optic systems to boost internet speeds, involving research in *photonics* and *signal processing*.
4. **Optical Sorting for Recycling:** Design systems using optics to sort recyclables by material type, needing knowledge in *optics* and *machine vision*.
5. **Portable OCT for Medical Imaging:** Develop small OCT devices for diagnosing conditions like eye diseases, focusing on *imaging technologies* and *software for analysis*.
# TOPIC
{topic}
# IDEAS
""")
    
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    chain = prompt | model
