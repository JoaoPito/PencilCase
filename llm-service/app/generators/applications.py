from app.generators.generator import Generator
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate
from decouple import config

class ApplicationsGenerator(Generator):
    name = 'Ideas'
    endpoint = '/applications'
    description = 'Suggests 3-5 real-world applications for the given topic.'

    prompt = ChatPromptTemplate.from_template("""You are a world-class professor in a prestigious university helping one of your students. 
Write 4 insightful application ideas for the topic provided. Your suggestions should be realistic and possible to be executed in the real world by a beginner in the field.
If research is required to execute any of these ideas, highlight which topics are needed. For each idea, write what problem in society it has the potential to solve.
Each idea SHOULD BE at most 1 paragraph long.
IF the student asks for something that is NOT a valid topic write the message "TOPIC NOT VALID".
ONLY WRITE YOUR IDEAS AND NOTHING ELSE.
NEVER WRITE ANY TITLES STARTING WITH '#'.
ALWAYS WRITE SOMETHING.
ALWAYS WRITE IN THE STUDENT'S LANGUAGE.
# EXAMPLE
## TOPIC
"Optics"
## IDEAS
**Smart Glasses for Vision Correction and AR**
Develop glasses that adjust focus automatically and integrate augmented reality for enhanced vision and functionality.
Helpful for people with changing vision (e.g., presbyopia) or those who need frequent changes in prescription.
Requires research in *adaptive optics* and *AR technologies*. 

**Non-Invasive Glucose Monitoring**
Create wearable optical sensors to track blood glucose levels through the skin, eliminating the inconvenience associated with traditional finger pricks for blood glucose monitoring, especially for people with diabetes.
You'll need to learn *near-infrared spectroscopy* and *biocompatible sensors*. 

**Optical Sorting for Recycling**
Design a computer-powered system to sort different items for recycling. You can sort them by color using a color sensor, for example. 
This can be a fun way to learn programming and explore solutions to physical problems.

**Portable OCT for Medical Imaging**
Develop small OCT devices for diagnosing conditions like eye diseases. Solves the need for more accessible and affordable diagnostic tools for eye diseases.
Needs exploration in *imaging technologies* and *software for analysis*.
# TOPIC
{topic}
# IDEAS
""")
    
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    chain = prompt | model
