"""
Adapted from https://github.com/venuv/langchain_yt_tools

CustomYTSearchTool searches YouTube videos related to a person
and returns a specified number of video URLs.
Input to this tool should be a comma separated list,
 - the first part contains a person name
 - and the second(optional) a number that is the
    maximum number of video results to return
    
Version with video titles, João Pito 20-05-2024
 """

from typing import Optional
from app.generators.generator import Generator

from langchain_core.callbacks import CallbackManagerForToolRun
from langchain_community.tools import BaseTool
from langchain.prompts import ChatPromptTemplate
from operator import itemgetter

class YouTubeSearchTool(BaseTool):
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
    
    def _format_to_md(self, results):
        markdown_str = ""
        for title, url, channel, _, _ in results:
            markdown_str = markdown_str + f"""- [{title}]({url}) - {channel}\n"""
        return markdown_str

    def _search(self, person: str, num_results: int) -> str:
        from youtube_search import YoutubeSearch

        results = YoutubeSearch(person, num_results).to_dict()
        results_list = [
            (video["title"], 
                "https://www.youtube.com" + video["url_suffix"],
                video["channel"],
                video["duration"],
                video["views"].split(" ")[0]
                ) for video in results
        ]
        return results_list

    def _run(
        self,
        query: str,
        run_manager: Optional[CallbackManagerForToolRun] = None,
    ) -> str:
        
        """Use the tool."""
        values = query.split(",")
        person = values[0]
        if len(values) > 1:
            num_results = int(values[1])
        else:
            num_results = 10
            
        results = self._search(person, num_results)
        return self._format_to_md(results)


class VideoGenerator(Generator):
    name = "Videos"
    endpoint = "/videos"
    description = "Adds a list of useful videos related to the topic."
    
    def format_topic(data):
        template = "{topic} beginner's course"
        topic = data.get("topic")
        return {"query": template.format(topic=topic)}
    
    youtube_tool = YouTubeSearchTool()
    
    chain = format_topic | youtube_tool