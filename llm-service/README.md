# PencilCase llm-service

This is responsible for providing LLM-based services. It's based on Langserve.

Run this service with `poetry run langchain serve`.

**More information about this API in the endpoint `/docs`.**

## Generators
This service is based on **Generators**, that binds endpoints to LLM chains to generate content.
For example, to generate an introduction to a study guide, a generator is used when the API is built to simplify binding an endpoint (like '/intro') to the LangChain chain responsible for generating it. 
The API builder builds a list of generators available, exposing its properties that can be used at the root endpoint.

### Attributes of a generator
- **Name:** This name will be displayed to the user
- **Description:** This is also showed to the user. It should communicate what this generator is and does.
- **Endpoint:** The API endpoint that will be used to call this generator's chain. A generator can be called with `[ENDPOINT]\invoke` with a *POST* request, default for Langserve.
- **Chain:** The LLM chain that will be executed when the endpoint is requested