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
          <PerftResults />
        </div>
      </div>
    </>
  );
};

export default Home;
