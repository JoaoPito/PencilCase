using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.API.Tests.Helpers;

public static class BlockTreeBuilder
{
    public static Block AddTopic(this Block root, string name)
    {
        var topic = new Block()
        {
            Name = name,
            Parent = root,
            ParentId = root.Id
        };
        root.Children.Add(topic);
        return topic;
    }

    public static Block AddNotebook(this Block root, string name, List<Block> cells)
    {
        var notebook = new Block()
        {
            Name = name,
            Parent = root,
            ParentId = root.Id,
            Type = BlockType.Notebook,
            Children = cells
        };
        root.Children.Add(notebook);
        return notebook;
    }

    public static Block AddDocument(this Block root, string name, List<Block> cells)
    {
        var document = new Block()
        {
            Name = name,
            Parent = root,
            ParentId = root.Id,
            Type = BlockType.Source,
            Children = cells
        };
        root.Children.Add(document);
        return document;
    }

    public static Block AddQuestion(this Block root, string question, int order, List<string>? answers = null)
    {
        if (root.Type != BlockType.Notebook)
            throw new ArgumentException("Invalid root block type");

        var questionCell = new Block()
        {
            Name = question,
            Parent = root,
            ParentId = root.Id,
            Type = BlockType.Cell,
            Children = new List<Block>(),
            Properties = new BlockProperties()
            {
                CellType = CellType.Question,
                Order = order
            }
        };

        if (answers != null)
        {
            foreach (var answer in answers)
            {
                questionCell.AddAnswer(answer, 0);
            }
        }

        root.Children.Add(questionCell);
        return questionCell;
    }
    
    public static Block AddAnswer(this Block question, string contents, int order)
    {
        var answer = new Block()
        {
            Name = contents,
            Parent = question,
            ParentId = question.Id,
            Type = BlockType.Cell,
            Properties = new BlockProperties()
            {
                CellType = CellType.Text,
                Order = order
            }
        };
        question.Children.Add(answer);
        return answer;
    }
}