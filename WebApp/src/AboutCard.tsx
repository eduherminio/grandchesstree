import React from "react";

const AboutCard: React.FC = () => {
  return (
    <>
      <div className="space-y-4 p-4 bg-gray-100 rounded-lg text-gray-700 flex flex-col justify-between items-center space-x-4">
        <span className="text-md font-bold">About</span>
        <span className="text-sm font-semibold">
          The Grand Chess Tree is a public, distributed effort to explore the
          depths of the game of chess. This project began from the enjoyment I
          found during the early stages of working on the{" "}
          <a
            className="font-medium text-blue-600 hover:underline"
            href="https://github.com/Timmoth/Sapling"
            target="_blank"
            rel="noopener noreferrer"
          >
            Saplings
          </a>{" "}
          move generator.
        </span>
        <span className="text-sm font-semibold">
          <a
            className="font-medium text-blue-600 hover:underline"
            href="https://www.chessprogramming.org/Perft_Results"
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
