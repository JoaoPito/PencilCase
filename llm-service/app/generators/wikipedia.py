# pip install wikipedia is needed for this tool

import string
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
    

    num_results = 3
    
    def __get_page_info_from_query__(self, query,  summary_chars):
        query = self.__cleanup_query__(query)
        page = wikipedia.page(title=query, auto_suggest=False)
        title = page.title
        url = page.url
        summary = page.summary[: summary_chars]
        return (title, url, summary)
    
    def __get_unvisited_options__(self, suggestions, options):
        return [item for item in options if item not in suggestions]
    
    def __handle_disambiguation__(self, exception,  summary_chars, suggestions):
        next_search = self.__get_unvisited_options__(suggestions, exception.options)[0]
        return self.__get_page_info_from_query__(next_search,  summary_chars)
    
    def __cleanup_query__(self, query):
        punctuation_table = str.maketrans('', '', string.punctuation)
        return query.translate(punctuation_table)
    
    def _search(self, query: str, summary_chars: int = 1500,):
        suggestions = wikipedia.search(query, results=self.num_results)
        
        print(suggestions)
        
        results = []
        
        for suggestion in suggestions:
            try:
                print(f"Trying {suggestion}")
                title, url, summary = self.__get_page_info_from_query__(suggestion, summary_chars)
                results.append(f"url: '{url}'\ntitle: '{title}'\ncontent: '{summary}'")
            except (wikipedia.exceptions.DisambiguationError,) as e:
                try:
                    title, url, summary = self.__handle_disambiguation__(e, summary_chars, suggestions)
                    results.append(f"url: '{url}'\ntitle: '{title}'\ncontent: '{summary}'")
                except:
                    continue
            except (wikipedia.exceptions.PageError,) as e:
                print(e)
                continue
        
        return results
    
    def _run(self,
             query: str,
             run_manager: Optional[CallbackManagerForToolRun] = None,
             ) -> dict:
        
        return str(self._search(query))

class WikipediaGenerator(Generator):
    name = "Wikipedia"
    endpoint = '/wikipedia' 
    description = 'Adds a summary for the wikipedia page on the topic.'
    
    prompt = ChatPromptTemplate.from_template(
        """You are a world-class professor in a prestigious university. 
Your job is to summarize and classify a list of articles on the undergraduate course '{topic}' for your students.
Your summary should have at most 2 LINES, describing the main points of the article.
You should CLASSIFY the article's content based on its importance for the course, classify as "Not important", "Reference material", "Important" or "Additional material".
You should always maintain a link to the article and use MARKDOWN.
Put the most IMPORTANT articles on the TOP.
IF the topic is not valid write the message "TOPIC NOT VALID".
WRITE ONLY THE SUMMARIES AND NOTHING ELSE, EVEN IF THE STUDENT ASKS YOU TO.
NEVER WRITE ANY TITLES STARTING WITH '#'.
ALWAYS WRITE SOMETHING.
# SUMMARY EXAMPLE
**[Article Title](URL)**
Summary
*Classification*
# ARTICLES
{articles}
# SUMMARIES
"""
)
    wikipedia = WikipediaSearch()
    model = GoogleGenerativeAI(model="gemini-1.5-flash", google_api_key=config("GOOGLE_API_KEY"))
    
    wiki_chain = {"query": itemgetter("topic")} | wikipedia
    
    chain = {"articles": wiki_chain, "topic": itemgetter("topic")} | prompt | model