import React from "react";

const AboutCard: React.FC = () => {
  return (
    <>
      <div className="space-y-4 p-4 bg-gray-100 rounded-lg text-gray-700 flex flex-col justify-between items-center space-x-4">
        <span className="text-md font-bold">About</span>
        <span className="text-sm font-semibold">
          The Grand Chess Tree is a global, community-driven effort to explore
          the vast complexity of chess.
        </span>
        <span className="text-sm">
          Perft (Performance Test) measures how many possible move sequences
          exist up to a given depth. Since chess is a branching game—each move
          creates new possibilities. At depth 1, White has 20 moves. At depth 2,
          there are 400 possible positions. By depth 3, it’s 8,902. Then 187K,
          4.8M, 3.2B… the numbers explode!
        </span>

        <span className="text-sm font-semibold">
          Perft is a crucial tool for testing chess engines, ensuring they
          analyze every move correctly. It also has a rich history, with
          enthusiasts pushing the limits of computation and ingenuity since the
          1980s.
        </span>
        <span className="text-sm">
          Perft doesn’t just count unique board positions—it tracks every
          possible sequence of moves, even if multiple paths lead to the same
          position.
        </span>
        <span className="text-sm">
          The table below breaks down results by move type, such as captures and
          promotions.
        </span>

        <span className="text-sm font-semibold">
          <a
            className="font-medium text-blue-600 hover:underline"
            href="https://www.chessprogramming.org/Perft"
            target="#"
          >
            Find out more.
          </a>
        </span>
      </div>
    </>
  );
};

export default AboutCard;
