from decouple import config
from fastapi import FastAPI
from fastapi.responses import RedirectResponse
from langserve import add_routes
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate

app = FastAPI()


@app.get("/")
async def redirect_root_to_docs():
    return RedirectResponse("/docs")

model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
prompt = ChatPromptTemplate.from_template("""You are a top-class professor. Write a Roadmap for a study guide on '{topic}'. 
The Roadmap should have main topics that need to be studied, ordered from most basic to advanced.
These topics can have 3-5 subtopics, ordered the same way.
If the student asks for something that is not a topic of study or if you cannot create it return an empty message.
WRITE ONLY THE ROADMAP AND NOTHING ELSE.
# EXAMPLE
1. **Topic 1:** 1-line description of the topic
    1.1. **Subtopic 1.1:** 1-line Description
    1.2. **Subtopic 1.2:** 1-line Description
    1.3. **Subtopic 1.3:** 1-line Description
2. **Topic 2:** 1-line description of the topic
    2.1. **Subtopic 2.1:** 1-line Description
    2.2. **Subtopic 2.2:** 1-line Description
    2.3. **Subtopic 2.3:** 1-line Description
    2.4. **Subtopic 2.4:** 1-line Description
# ROADMAP""")

chain = prompt | model

add_routes(app, chain, path="/roadmap")

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8000)
