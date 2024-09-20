# pip install wikipedia is needed for this tool

from app.generators.generator import Generator
from langchain_google_genai import GoogleGenerativeAI
from langchain.prompts import ChatPromptTemplate
from decouple import config

import wikipedia

from typing import Optional
from langchain_core.callbacks import CallbackManagerForToolRun
from langchain_community.tools import BaseTool

from operator import itemgetter

class WikipediaSearch(BaseTool):
    """Tool that queries YouTube."""

    name: str = "youtube_search"
    description: str = (
        "search for youtube videos associated with a person. "
        "the input to this tool should be a comma separated list, "
        "the first part contains a person name and the second a "
        "number that is the maximum number of video results "
        "to return aka num_results. the second part is optional. "
        "Never use commas in the first part"
    )
    
    def __get_page_info_from_query__(self, query, max_sentences):
        page = wikipedia.page(query)
        title = page.title
        url = page.url
        summary = wikipedia.summary(query, sentences=max_sentences)
        return (title, url, summary)
    
    def _run(self,
             query: str,
             num_results: int = 3,
             max_sentences: int = 5,
             run_manager: Optional[CallbackManagerForToolRun] = None,
             ) -> dict:
        
        suggestions = wikipedia.search(query, results=num_results)
        
        print(suggestions)
        
        results = []
        for suggestion in suggestions:
            try:
                title, url, summary = self.__get_page_info_from_query__(suggestion, max_sentences=max_sentences)
            except (wikipedia.exceptions.DisambiguationError,wikipedia.exceptions.PageError,) as e:
                print(e)
                continue
            
            results.append(f"url: '{url}'\ntitle: '{title}'\ncontent: '{summary}'")
        
        return results

class WikipediaGenerator(Generator):
    name = "Wikipedia"
    endpoint = '/wikipedia' 
    description = 'Adds a summary for the wikipedia page on the topic.'
    
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
    
    wikipedia = WikipediaSearch()
    num_results = 3
    max_sentences = 5
    
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    chain = {"query": itemgetter("topic")} | wikipedia