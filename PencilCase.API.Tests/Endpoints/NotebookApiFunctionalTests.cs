using Moq;
using PencilCase.LLM.Agents.Providers;
using PencilCase.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Endpoints;

[TestFixture]
public class NotebookApiFunctionalTests
{
    [Test]
    public void UserGetsDocumentsFromRagAndUsesOnLlm()
    {
        // Carlos is studying mathematics and just started to use pencilcase,
        // It's the end of the semester, so he is studying for multiple tests
        // so he adds some PDFs to his workspace, but not all PDFs are for math, since he also studies a psychology course
        // He, then, creates a new notebook and starts adding cells to it
        // he submits a cell he was working on
        // After some time loading, pencilcase gets the relevant documents to the question and shows them to Carlos
        // Then, pencilcase sends them to the LLM using the appropriate endpoint
        // It loads for a couple of seconds, but pencilcase finally shows him the answer to his question
        // He tries to change the question a little bit, since it wasn't helping much
        // Again, he waits for the answer to his question a little bit. pencilcase shows him the answer.
        // He then decides to delete this cell, since he felt the question wasn't really very useful to him
        // 
    }
}