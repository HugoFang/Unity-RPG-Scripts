using UnityEngine;

namespace RPG.Conversing {
  [System.Serializable]
  public struct Dialogue {
    [SerializeField] private string name;
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogueLines;

    public string[] DialogueLines => dialogueLines;
    public string Name => name;
  }
}
