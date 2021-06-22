all:
	make full

full:
	rm -f ../AdditionalQuests.zip
	zip ../AdditionalQuests.zip -r ./bin SubModule.xml README.md
