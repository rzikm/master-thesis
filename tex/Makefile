TEX_DIR=${CURDIR}
TEX_SRC=$(wildcard $(TEX_DIR)/*.tex)

IMG_DIR=img
IMG_SRC=$(wildcard $(IMG_DIR)/*.svg)
IMG_OUT=$(patsubst %.svg, %.pdf_tex, $(IMG_SRC))
IMG_PDF=$(patsubst %.svg, %.pdf, $(IMG_SRC))

all: thesis.pdf abstract.pdf

# LaTeX must be run multiple times to get references right
thesis.pdf: $(TEX_DIR)/thesis.tex $(TEX_SRC) bibliography.bib $(IMG_OUT)
	pdflatex $<
	bibtex thesis
	makeglossaries thesis
	pdflatex $<
	pdflatex $<

abstract.txt: thesis.pdf

abstract.pdf: abstract.tex abstract.xmpdata abstract.txt
	pdflatex $<

$(IMG_DIR)/%.pdf_tex: $(IMG_DIR)/%.svg
	inkscape --export-filename $(patsubst %.svg, %.pdf, $<) --export-latex $<

images: $(IMG_OUT)

clean:
	rm -f *.log *.dvi *.aux *.toc *.lof *.lot *.out *.bbl *.blg *.xmpi *.glg *.glo *.gls *.ist *.lol
	rm -f thesis.pdf abstract.pdf abstract.txt
	rm -f thesis.xmpdata
	rm -f $(IMG_PDF) $(IMG_OUT)
