import React from "react";
import NavBar from "./NavBar";
import AboutCard from "./AboutCard";
import PerftResults from "./PerftResults";

const Home: React.FC = () => {
  return (
    <>
      <div>
        <NavBar />

        <div className="flex flex-col m-4 space-y-4 mt-20">
        <AboutCard />
          <div className="space-y-4 p-4 bg-gray-100 rounded-lg text-gray-700 flex flex-col justify-between items-center space-x-4">
              <span className="text-md font-bold">Want to get involved?</span>
              <div className="flex-1 flex flex-col items-center justify-center space-y-4">
                <span className="text-sm font-semibold">
                  If you're interested in volunteering computing resources or
                  collaborating on the project
                </span>
                <span className="text-sm font-semibold">
                  <a
                    className="font-medium text-blue-600 hover:underline"
                    href="https://discord.gg/cTu3aeCZVe"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    join the Discord server!
                  </a>
                </span>
              </div>
            </div>
          <PerftResults />
        </div>
      </div>
    </>
  );
};

export default Home;
