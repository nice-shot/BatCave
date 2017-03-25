using Infra.UI;

namespace BatCave.UI {
public class ScoreCounter : CounterText {
    protected override int GetTarget() {
        return Game.instance == null ? 0 : Game.instance.Score;
    }
}
}
